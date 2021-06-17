using Sandbox;
using RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;
using RTS.Buildings;
using System.Linq;
using System;
using Gamelib.Extensions;
using Sandbox.UI;

namespace RTS
{
	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable
	{
		public Dictionary<ResourceType, int> Carrying { get; private set; }
		[Net, Local] public float GatherProgress { get; private set; }
		[Net, Local] public bool IsGathering { get; private set; }
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
		public float Speed { get; private set; }
		public Entity Target { get; private set; }
		public TimeSince LastGatherTime { get; private set; }
		public ResourceEntity LastResourceEntity { get; private set; }
		public ResourceType LastResourceType { get; private set; }
		public Vector3 LastResourcePosition { get; private set; }
		public Vector3 InputVelocity { get; private set; }
		public float TargetRange { get; private set; }
		public float WishSpeed { get; private set; }

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		public EntityHudBar GatherBar { get; private set; }
		#endregion

		public NavSteer Steer;

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

		public override void ClientSpawn()
		{
			Circle = new();
			Circle.Size = CollisionBounds.Size.Length * 0.8f;
			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			if ( Player.IsValid() && Player.IsLocalPawn )
				FogManager.Instance.AddViewer( this );
			else
				FogManager.Instance.AddCullable( this );

			base.ClientSpawn();
		}

		public void Attack( Entity target, bool autoFollow = true )
		{
			Target = target;
			TargetRange = Item.AttackRange;
			FollowTarget = autoFollow;
			IsGathering = false;
		}

		public void MoveTo( Vector3 position )
		{
			Target = null;
			Steer ??= new();
			Steer.Target = position;
			FollowTarget = false;
			IsGathering = false;
		}

		public void Deposit( BuildingEntity building )
		{
			Target = building;
			Steer ??= new();
			Steer.Target = building.Position;
			FollowTarget = true;
			IsGathering = false;
			TargetRange = Item.InteractRange;
		}

		public void Gather( ResourceEntity resource)
		{
			Target = resource;
			Steer ??= new();
			Steer.Target = resource.Position;
			FollowTarget = true;
			TargetRange = Item.InteractRange;
			IsGathering = false;
			LastResourceType = resource.Resource;
			LastResourceEntity = resource;
			LastResourcePosition = resource.Position;
		}

		public void Construct( BuildingEntity building )
		{
			Target = building;
			Steer ??= new();
			Steer.Target = building.Position;
			FollowTarget = true;
			IsGathering = false;
			TargetRange = Item.InteractRange;
		}

		public void ClearTarget()
		{
			Steer = null;
			Target = null;
			IsGathering = false;
			FollowTarget = false;
		}

		public void MakeVisible( bool isVisible )
		{
			TargetAlpha = isVisible ? 1f : 0f;
			UI.SetVisible( isVisible );
		}

		public ModelEntity AttachClothing( string modelName )
		{
			var entity = new ModelEntity();

			entity.SetModel( modelName );
			entity.SetParent( this, true );
			entity.EnableShadowInFirstPerson = true;
			entity.EnableHideInFirstPerson = true;

			Clothing.Add( entity );

			return entity;
		}

		public void RemoveClothing()
		{
			Clothing.ForEach( ( entity ) => entity.Delete() );
			Clothing.Clear();
		}

		public bool IsEnemy( ISelectable other )
		{
			return (other.Player != Player);
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				FogManager.Instance.RemoveViewer( this );
				FogManager.Instance.RemoveCullable( this );
			}

			base.OnDestroy();
		}

		protected override void OnItemChanged( BaseUnit item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
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
				Weapon.Unit = this;

				var attachment = GetAttachment( "weapon", true );
				
				if ( attachment.HasValue )
				{
					Weapon.SetParent( this );
					Weapon.Position = attachment.Value.Position;
				}
				else
				{
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

			ClearTarget();
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

			ClearTarget();
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
					Steer.Target = Target.Position;
				}
				else if ( !IsSelected )
				{
					if ( Target is ResourceEntity )
						FindTargetResource();
					else
						FindTargetUnit();
				}

				if ( Steer != null )
				{
					Steer.Tick( Position );

					if ( !Steer.Output.Finished )
					{
						var control = GroundEntity != null ? 200 : 10;

						InputVelocity = Steer.Output.Direction.Normal * Speed;
						var vel = Steer.Output.Direction.WithZ( 0 ).Normal * Time.Delta * control;
						Velocity = Velocity.AddClamped( vel, Speed );

						SetAnimLookAt( "aim_head", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
						SetAnimLookAt( "aim_body", EyePos + Steer.Output.Direction.WithZ( 0 ) * 10 );
						SetAnimFloat( "aim_body_weight", 0.25f );
					}
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
				var targetDirection = Target.Position - Position;
				var targetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

				Rotation = Rotation.Lerp( Rotation, targetRotation, Time.Delta * 15f );

				if ( Rotation.Distance( targetRotation ).AlmostEqual( 0f, 0.1f ) )
				{
					if ( Target is BuildingEntity building && building.Player == Player )
					{
						TickConstruct( building );
					}
					else if ( Target is ResourceEntity resource )
					{
						TickGather( resource );
					}
					else if ( Weapon.IsValid() && Weapon.CanAttack() )
					{
						Weapon.Attack( Target );
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

		private void TickConstruct( BuildingEntity building )
		{
			if ( building.IsUnderConstruction )
			{
				building.Health += (building.Item.MaxHealth / building.Item.BuildTime * Time.Delta);
				building.Health = building.Health.Clamp( 0f, building.Item.MaxHealth );

				if ( building.Health == building.Item.MaxHealth )
				{
					building.FinishConstruction();
					ClearTarget();
				}
				else
				{
					building.UpdateConstruction();
				}
			}
			else if ( building.CanDepositResources )
			{
				foreach ( var kv in Carrying )
				{
					Player.GiveResource( kv.Key, kv.Value );
				}

				Carrying.Clear();

				FindTargetResource();
			}
			else
			{
				ClearTarget();
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
			if ( Circle.IsValid() && Player.IsValid() )
			{
				if ( Player.IsLocalPawn && IsSelected )
					Circle.Color = Color.White;
				else
					Circle.Color = Player.TeamColor;
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
	}
}

