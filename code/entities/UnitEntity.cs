using Sandbox;
using Facepunch.RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;
using Facepunch.RTS.Buildings;
using System.Linq;
using System;
using Gamelib.Extensions;
using Sandbox.UI;
using Gamelib.FlowField;
using Gamelib.Math;

namespace Facepunch.RTS
{
	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable, IDamageable
	{
		public override bool HasSelectionGlow => false;
		public Dictionary<ResourceType, int> Carrying { get; private set; }
		[Net, Local] public float GatherProgress { get; private set; }
		[Net, Local] public bool IsGathering { get; private set; }
		[Net, Local] public bool IsInsideBuilding { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public float LineOfSight { get; private set; }
		[Net, Local] public int Kills { get; set; }
		public override bool CanMultiSelect => true;
		public List<ModelEntity> Clothing => new();
		public UnitCircle Circle { get; private set; }
		public TimeSince LastAttackTime { get; set; }
		public bool HasBeenSeen { get; set; }
		public bool FollowTarget { get; private set; }
		public float TargetAlpha { get; private set; }
		public Vector3? TargetPosition { get; private set; }
		public float Speed { get; private set; }
		public Entity Target { get; private set; }
		public TimeSince LastGatherTime { get; private set; }
		public ResourceEntity LastResourceEntity { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public ResourceType LastResourceType { get; private set; }
		public Vector3 LastResourcePosition { get; private set; }
		public FlowField FlowField { get; private set; }
		public Vector3 InputVelocity { get; private set; }
		public float? SpinSpeed { get; private set; }
		public float TargetRange { get; private set; }
		public float WishSpeed { get; private set; }

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		public EntityHudBar GatherBar { get; private set; }
		#endregion

		public UnitEntity() : base()
		{
			Tags.Add( "unit", "selectable" );

			if ( IsServer )
			{
				Carrying = new();
			}
		}

		public bool CanConstruct => Item.CanConstruct;

		public bool CanGather( ResourceType type )
		{
			return Item.Gatherables.Contains( type );
		}

		public bool IsTargetInRange()
		{
			if ( !Target.IsValid() ) return false;

			if ( Target is ModelEntity entity )
			{
				// We can try to see if our range overlaps the bounding box of the target.
				var targetBounds = entity.CollisionBounds + entity.Position;

				if ( targetBounds.Overlaps( Position, TargetRange ) )
					return true;
			}

			return (Target.IsValid() && Target.Position.Distance( Position ) < TargetRange);
		}

		public bool TakeFrom( ResourceEntity resource )
		{
			if ( resource.Stock <= 0 ) return false;

			if ( Carrying.TryGetValue( resource.Resource, out var carrying ) )
			{
				if ( carrying < resource.MaxCarry )
					Carrying[resource.Resource] += 1;
				else
					return false;
			}
			else
			{
				Carrying[resource.Resource] = 1;
			}

			resource.Stock -= 1;

			if ( resource.Stock <= 0 )
				resource.Delete();

			return true;
		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdoll( Velocity, LastDamageTaken.Flags, LastDamageTaken.Position, LastDamageTaken.Force, GetHitboxBone( LastDamageTaken.HitboxIndex ) );
		}

		public override void TakeDamage( DamageInfo info )
		{
			LastDamageTaken = info;
			base.TakeDamage( info );
		}

		public override void ClientSpawn()
		{
			Circle = new();
			Circle.Size = CollisionBounds.Size.Length * 0.8f;
			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			if ( Player.IsValid() && Player.IsLocalPawn )
				RTS.Fog.AddViewer( this );
			else
				RTS.Fog.AddCullable( this );

			base.ClientSpawn();
		}

		public void DoImpactEffects( TraceResult trace )
		{
			var impactEffects = Item.ImpactEffects;
			var particleName = impactEffects[Rand.Int( 0, impactEffects.Count - 1 )];

			if ( particleName != null )
			{
				var particles = Particles.Create( particleName, trace.EndPos );
				particles.SetForward( 0, trace.Normal );
			}
		}

		public void CreateDamageDecals( Vector3 position )
		{
			var damageDecals = Item.DamageDecals;

			if ( damageDecals.Count == 0 ) return;

			var randomDecalName = damageDecals[Rand.Int( 0, damageDecals.Count - 1 )];
			var decalMaterial = Material.Load( randomDecalName );
			var decalRotation = Rotation.LookAt( Vector3.Up ) * Rotation.FromAxis( Vector3.Forward, Rand.Float( 0f, 360f ) );
			var randomSize = Rand.Float( 50f, 100f );
			var trace = Trace.Ray( position, position + Vector3.Down * 100f ).Ignore( this ).Run();

			Decals.Place( decalMaterial, trace.EndPos, new Vector3( randomSize, randomSize, 4f ), decalRotation );
		}

		public void Attack( Entity target, bool autoFollow = true )
		{
			ResetTarget();

			Target = target;
			TargetRange = Item.AttackRange;
			FollowTarget = autoFollow;

			OnTargetChanged();
		}

		public void MoveTo( Vector3 position )
		{
			if ( FlowField != null )
			{
				RTS.Game.Pathfinder.Finish( FlowField );
			}

			FlowField = RTS.Game.Pathfinder.Request( Position, position );

			ResetTarget( position);
			OnTargetChanged();
		}

		public void Occupy( BuildingEntity building )
		{
			ResetTarget( building.Position );

			Target = building;
			FollowTarget = true;
			TargetRange = Item.InteractRange;

			OnTargetChanged();
		}


		public void Deposit( BuildingEntity building )
		{
			ResetTarget( building.Position );

			Target = building;
			FollowTarget = true;
			TargetRange = Item.InteractRange;

			OnTargetChanged();
		}

		public void Gather( ResourceEntity resource)
		{
			ResetTarget( resource.Position );

			Target = resource;
			FollowTarget = true;
			TargetRange = Item.InteractRange;
			LastResourceType = resource.Resource;
			LastResourceEntity = resource;
			LastResourcePosition = resource.Position;

			OnTargetChanged();
		}

		public void Construct( BuildingEntity building )
		{
			ResetTarget( building.Position );

			Target = building;
			FollowTarget = true;
			TargetRange = Item.InteractRange;

			OnTargetChanged();
		}

		public void ClearTarget()
		{
			Target = null;
			TargetPosition = null;
			IsGathering = false;
			FollowTarget = false;
			OnTargetChanged();
		}

		public float LookAtEntity( Entity target, float? interpolation = null )
		{
			var targetDirection = target.Position - Position;
			var targetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

			if ( interpolation.HasValue )
				Rotation = Rotation.Lerp( Rotation, targetRotation, interpolation.Value );
			else
				Rotation = targetRotation;

			return Rotation.Distance( targetRotation );
		}

		public void MakeVisible( bool isVisible )
		{
			TargetAlpha = isVisible ? 1f : 0f;
			UI.SetVisible( isVisible );
		}

		public ModelEntity AttachClothing( string modelName )
		{
			var entity = new Clothes();

			entity.SetModel( modelName );
			entity.SetParent( this, true );

			Clothing.Add( entity );

			return entity;
		}

		public void RemoveClothing()
		{
			Clothing.ForEach( ( entity ) => entity.Delete() );
			Clothing.Clear();
		}

		public virtual void OnEnterBuilding( BuildingEntity building )
		{
			Deselect();
			SetParent( this );
			IsInsideBuilding = true;
			EnableDrawing = false;
			EnableAllCollisions = false;
		}

		public virtual void OnLeaveBuilding( BuildingEntity building )
		{
			SetParent( null );
			IsInsideBuilding = false;
			EnableDrawing = true;
			EnableAllCollisions = true;
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				Circle?.Delete();
				RTS.Fog.RemoveViewer( this );
				RTS.Fog.RemoveCullable( this );
			}
			else
			{
				if ( Player.IsValid() )
					Player.TakePopulation( Item.Population );
			}

			base.OnDestroy();
		}

		protected override void OnItemChanged( BaseUnit item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );

				var materialGroups = MaterialGroupCount;

				if ( materialGroups > 0 )
					SetMaterialGroup( Rand.Int( 0, materialGroups ) );
			}

			foreach ( var clothes in item.Clothing )
			{
				AttachClothing( clothes );
			}

			Speed = item.Speed;
			Health = item.MaxHealth;
			MaxHealth = item.MaxHealth;
			EyePos = Position + Vector3.Up * 64;
			LineOfSight = item.LineOfSight;
			CollisionGroup = CollisionGroup.Player;
			EnableHitboxes = true;

			SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );

			if ( !string.IsNullOrEmpty( item.Weapon ) )
			{
				Weapon = Library.Create<Weapon>( item.Weapon );
				Weapon.Attacker = this;

				var attachment = GetAttachment( "weapon", true );
				
				if ( attachment.HasValue )
				{
					Weapon.SetParent( this );
					Weapon.Position = attachment.Value.Position;
				}
				else
				{
					Weapon.Position = Position;
					Weapon.SetParent( this, true );
				}
			}

			base.OnItemChanged( item );
		}

		protected virtual void Move( float timeDelta )
		{
			var bbox = BBox.FromHeightAndRadius( 64, 4 );

			MoveHelper move = new( Position, Velocity );
			move.MaxStandableAngle = 50;
			move.Trace = move.Trace.Ignore( this ).Size( bbox );

			if ( !Velocity.IsNearlyZero( 0.001f ) )
			{
				move.TryUnstuck();
				move.TryMoveWithStep( timeDelta, 30 );
			}

			var tr = move.TraceDirection( Vector3.Down * 10.0f );

			if ( move.IsFloor( tr ) )
			{
				GroundEntity = tr.Entity;

				if ( !tr.StartedSolid )
				{
					move.Position = tr.EndPos;
				}

				move.Velocity -= InputVelocity;
				move.ApplyFriction( tr.Surface.Friction * 200.0f, timeDelta );
				move.Velocity += InputVelocity;
			}
			else
			{
				GroundEntity = null;
				move.Velocity += Vector3.Down * 900 * timeDelta;
			}

			Position = move.Position;
			Velocity = move.Velocity;
		}

		private void ResetTarget( Vector3? position = null )
		{
			Target = null;
			TargetPosition = position.Value;
			IsGathering = false;
			FollowTarget = false;
		}

		private void FindTargetResource()
		{
			// If our last resource entity is valid just use that.
			if ( LastResourceEntity.IsValid() )
			{
				Gather( LastResourceEntity );
				return;
			}

			var entities = Physics.GetEntitiesInSphere( LastResourcePosition, 1000f );

			foreach ( var entity in entities )
			{
				if ( entity is ResourceEntity resource && resource.Resource == LastResourceType )
				{
					Gather( resource );
					return;
				}
			}
		}

		private void FindResourceDepo()
		{
			var buildings = Player.GetBuildings().Where( i => i.Item.CanDepositResources );
			var closestDepo = (BuildingEntity)null;
			var closestDistance = 0f;

			foreach ( var depo in buildings )
			{
				var distance = depo.Position.Distance( Position );

				if ( !closestDepo.IsValid() || distance < closestDistance )
				{
					closestDepo = depo;
					closestDistance = distance;
				}
			}

			if ( closestDepo.IsValid() )
				Deposit( closestDepo );
			else
				ClearTarget();
		}

		private void FindTargetUnit()
		{
			var entities = Physics.GetEntitiesInSphere( Position, Item.AttackRange );
			
			foreach ( var entity in entities )
			{
				if ( entity is UnitEntity unit && IsEnemy( unit ) )
				{
					Attack( unit, false );
					return;
				}
			}
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			InputVelocity = 0;

			var isTargetInRange = IsTargetInRange();

			if ( !Target.IsValid() || !isTargetInRange )
			{
				if ( Target.IsValid() && FollowTarget )
				{
					TargetPosition = Target.Position;
				}
				else if ( !IsSelected )
				{
					if ( Target is ResourceEntity )
						FindTargetResource();
					else if ( Weapon.IsValid() )
						FindTargetUnit();
				}

				if ( TargetPosition.HasValue )
				{
					var targetPosition = TargetPosition.Value;
					var pathDirection = (targetPosition - Position).Normal;

					if ( FlowField != null )
					{
						var node = FlowField.GetNodeAtWorld( Position );
						var chunk = FlowField.GetChunkAtWorld( Position );
						var targetNode = FlowField.GetNodeAtWorld( targetPosition );

						/*
						if ( !node.IsWalkable )
							node.Debug( Color.Red, 0f );
						else
							node.Debug( Color.Green, 0f );
						*/

						foreach ( var n in chunk.Nodes )
						{
							var scale = (1f / 10f) * n.GetDistance();
							//n.Debug( Color.Lerp( Color.Green, Color.Red, scale ) );
						}

						if ( targetNode == node )
						{
							RTS.Game.Pathfinder.Finish( FlowField );
						}
						else
						{
							pathDirection = node.GetDirection();
						}

						/*
						foreach ( var p in FlowField.CurrentPortals )
						{
							foreach ( var n in p.ChunkNodes )
							{
								n.Debug( Color.Magenta );
							}
						}
						*/
					}

					if ( Position.Distance( targetPosition ) > 10f )
					{
						var control = GroundEntity != null ? 200f : 10f;

						InputVelocity = pathDirection.Normal * Speed;
						var velocity = pathDirection.WithZ( 0 ).Normal * Time.Delta * control;
						Velocity = Velocity.AddClamped( velocity, Speed );

						SetAnimLookAt( "aim_head", EyePos + pathDirection.WithZ( 0 ) * 10 );
						SetAnimLookAt( "aim_body", EyePos + pathDirection.WithZ( 0 ) * 10 );
						SetAnimFloat( "aim_body_weight", 0.25f );
					}
					else
					{
						TargetPosition = null;
					}
				}
				else
				{
					Velocity = 0;
				}

				Move( Time.Delta );

				var walkVelocity = Velocity.WithZ( 0 );

				if ( walkVelocity.Length > 1 )
				{
					Rotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
				}
			}
			else
			{
				var lookAtDistance = 0f;

				if ( SpinSpeed.HasValue )
					Rotation = Rotation.FromYaw( Rotation.Yaw() + SpinSpeed.Value * Time.Delta );
				else
					lookAtDistance = LookAtEntity( Target, Time.Delta * 15f );

				if ( SpinSpeed.HasValue || lookAtDistance.AlmostEqual( 0f, 0.1f ) )
				{
					if ( Target is BuildingEntity building && building.Player == Player )
					{
						if ( building.IsUnderConstruction )
							TickConstruct( building );
						else if ( building.CanDepositResources )
							DepositResources();
						else if ( building.CanOccupyUnits )
							TickOccupy( building );
						else
							ClearTarget();
					}
					else if ( Target is ResourceEntity resource )
					{
						TickGather( resource );
					}
					else if ( Weapon.IsValid() && Weapon.CanAttack() )
					{
						Weapon.Attack();
					}
				}
			}

			if ( Weapon.IsValid() )
				SetAnimInt( "holdtype", Weapon.HoldType );
			else
				SetAnimInt( "holdtype", 0 );

			WishSpeed = WishSpeed.LerpTo( InputVelocity.Length, 10f * Time.Delta );

			SetAnimBool( "b_grounded", true );
			SetAnimBool( "b_noclip", false );
			SetAnimBool( "b_swim", false );
			SetAnimFloat( "forward", Vector3.Dot( Rotation.Forward, InputVelocity ) );
			SetAnimFloat( "sideward", Vector3.Dot( Rotation.Right, InputVelocity ) );
			SetAnimFloat( "wishspeed", WishSpeed );
			SetAnimFloat( "walkspeed_scale", 2.0f / 10.0f );
			SetAnimFloat( "runspeed_scale", 2.0f / 320.0f );
			SetAnimFloat( "duckspeed_scale", 2.0f / 80.0f );
		}

		private void TickOccupy( BuildingEntity building )
		{
			if ( building.OccupyUnit( this ) ) ClearTarget();
		}

		private void DepositResources()
		{
			foreach ( var kv in Carrying )
			{
				Player.GiveResource( kv.Key, kv.Value );
			}

			Carrying.Clear();

			FindTargetResource();
		}

		private void TickConstruct( BuildingEntity building )
		{
			building.Health += (building.MaxHealth / building.Item.BuildTime * Time.Delta);
			building.Health = building.Health.Clamp( 0f, building.Item.MaxHealth );

			SpinSpeed = (building.MaxHealth / building.Health) * 200f;
				
			if ( building.Health == building.Item.MaxHealth )
			{
				LookAtEntity( building );
				building.FinishConstruction();
				ClearTarget();
			}
			else
			{
				building.UpdateConstruction();
			}
		}

		private void TickGather( ResourceEntity resource )
		{
			if ( LastGatherTime < resource.GatherTime ) return;

			TakeFrom( resource );

			LastGatherTime = 0;
			IsGathering = true;

			if ( !Carrying.TryGetValue( resource.Resource, out var carrying ) )
				return;

			GatherProgress = (1f / resource.MaxCarry) * carrying;

			if ( carrying < resource.MaxCarry ) return;

			// We're full, let's deposit that shit.
			FindResourceDepo();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( IsInsideBuilding )
			{
				Circle.EnableDrawing = false;
				EnableDrawing = false;

				return;
			}

			if ( Circle.IsValid() && Player.IsValid() )
			{
				if ( Player.IsLocalPawn && IsSelected )
					Circle.Color = Color.White;
				else
					Circle.Color = Player.TeamColor;

				Circle.EnableDrawing = true;
			}

			if ( IsLocalPlayers ) return;

			var lerpSpeed = Time.Delta * 8f;

			RenderAlpha = RenderAlpha.LerpTo( TargetAlpha, lerpSpeed );

			for ( var i = 0; i < Children.Count; i++ )
			{
				if ( Children[i] is ModelEntity child )
				{
					child.RenderAlpha = child.RenderAlpha.LerpTo( TargetAlpha, lerpSpeed );
				}
			}

			if ( Circle.IsValid() )
			{
				Circle.Alpha = Circle.Alpha.LerpTo( TargetAlpha, lerpSpeed );
			}

			EnableDrawing = (RenderAlpha > 0f);
		}

		protected virtual void OnTargetChanged()
		{
			if ( Weapon.IsValid() )
				Weapon.Target = Target;

			SpinSpeed = null;
		}

		protected override void AddHudComponents()
		{
			HealthBar = UI.AddChild<EntityHudBar>( "health" );

			if ( IsLocalPlayers )
				GatherBar = UI.AddChild<EntityHudBar>( "gather" );

			base.AddHudComponents();
		}

		protected override void UpdateHudComponents()
		{
			if ( Health <= MaxHealth * 0.9f )
			{
				HealthBar.Foreground.Style.Width = Length.Fraction( Health / MaxHealth );
				HealthBar.Foreground.Style.Dirty();
				HealthBar.SetClass( "hidden", false );
			}
			else
			{
				HealthBar.SetClass( "hidden", true );
			}

			if ( IsGathering && IsLocalPlayers )
			{
				GatherBar.Foreground.Style.Width = Length.Fraction( GatherProgress );
				GatherBar.Foreground.Style.Dirty();
				GatherBar.SetClass( "hidden", false );
			}
			else
			{
				GatherBar?.SetClass( "hidden", true );
			}

			base.UpdateHudComponents();
		}

		[ClientRpc]
		private void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
		{
			Ragdoll.From( this, velocity, damageFlags, forcePos, force, bone ).FadeOut( 10f );
		}
	}
}

