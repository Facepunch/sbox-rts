using Sandbox;
using Facepunch.RTS.Units;
using System.Collections.Generic;
using Gamelib.Nav;
using Facepunch.RTS.Buildings;
using System.Linq;
using System;
using Gamelib.Extensions;
using Sandbox.UI;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Grid;

namespace Facepunch.RTS
{
	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable, IDamageable, IFlockAgent
	{
		private struct AnimationValues
		{
			public float Walking;

			public void Lerp( AnimEntity entity, string name, float value )
			{
				entity.SetAnimFloat( name, entity.GetAnimFloat( name ).LerpTo( value, Time.Delta * 10f ) );
			}
		}

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
		public MoveGroup MoveGroup { get; private set; }
		public Vector3 InputVelocity { get; private set; }
		public float? SpinSpeed { get; private set; }
		public float TargetRange { get; private set; }

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		public EntityHudBar GatherBar { get; private set; }
		#endregion

		private AnimationValues _animationValues;
		private FlockSettings _flockSettings;

		public UnitEntity() : base()
		{
			Tags.Add( "unit", "selectable" );

			if ( IsServer )
			{
				Carrying = new();
			}

			_flockSettings = new FlockSettings()
			{
				Radius = 200f,
				MaxSpeed = 300f,
				MaxForce = 300f
			};

			// Don't collide with anything but static shit.
			CollisionGroup = CollisionGroup.Debris;
		}

		public bool CanConstruct => Item.CanConstruct;
		public FlockSettings FlockSettings => _flockSettings;

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

				if ( targetBounds.Overlaps( Position, TargetRange * 2f ) )
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

			Decals.Place( decalMaterial, trace.Entity, trace.Bone, trace.EndPos, new Vector3( randomSize, randomSize, 4f ), decalRotation );
		}

		public bool IsInMoveGroup( UnitEntity other )
		{
			return (other.MoveGroup == MoveGroup);
		}

		public void Attack( Entity target, bool autoFollow = true )
		{
			ResetTarget();
			Target = target;
			TargetRange = Item.AttackRange;
			FollowTarget = autoFollow;
			OnTargetChanged();
		}

		public void MoveTo( MoveGroup group )
		{
			ResetTarget();
			MoveGroup = group;
			OnTargetChanged();
		}

		public void MoveTo( Vector3 position )
		{
			ResetTarget();
			MoveGroup = CreateSingleMoveGroup( position );
			OnTargetChanged();
		}

		public MoveGroup CreateSingleMoveGroup( Vector3 destination )
		{
			return new MoveGroup( new List<UnitEntity>() { this }, destination );
		}

		public MoveGroup CreateSingleMoveGroup( List<Vector3> destinations )
		{
			return new MoveGroup( new List<UnitEntity>() { this }, destinations );
		}

		public void Occupy( BuildingEntity building, MoveGroup moveGroup = null )
		{
			if ( moveGroup == null )
				moveGroup = CreateSingleMoveGroup( GetDestinations( building ) );

			ResetTarget();
			Target = building;
			MoveGroup = moveGroup;
			FollowTarget = true;
			TargetRange = Item.InteractRange;
			OnTargetChanged();
		}

		public void Deposit( BuildingEntity building, MoveGroup moveGroup = null )
		{
			if ( moveGroup == null )
				moveGroup = CreateSingleMoveGroup( GetDestinations( building ) );

			ResetTarget();
			Target = building;
			MoveGroup = moveGroup;
			FollowTarget = true;
			TargetRange = Item.InteractRange;
			OnTargetChanged();
		}

		public void Gather( ResourceEntity resource, MoveGroup moveGroup = null )
		{
			if ( moveGroup == null )
				moveGroup = CreateSingleMoveGroup( GetDestinations( resource ) );

			ResetTarget();
			Target = resource;
			MoveGroup = MoveGroup;
			FollowTarget = true;
			TargetRange = Item.InteractRange;
			LastResourceType = resource.Resource;
			LastResourceEntity = resource;
			LastResourcePosition = resource.Position;
			OnTargetChanged();
		}

		public void Construct( BuildingEntity building, MoveGroup moveGroup = null )
		{
			if ( moveGroup == null )
				moveGroup = CreateSingleMoveGroup( GetDestinations( building ) );

			ResetTarget();
			Target = building;
			MoveGroup = moveGroup;
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
			ClearMoveGroup();
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

		public List<Vector3> GetDestinations( ModelEntity model )
		{
			var potentialTiles = new List<Vector3>();
			var collisionSize = model.CollisionBounds.Size.Length * 0.4f;
			var possibleLocations = new List<GridWorldPosition>();

			RTS.Path.Pathfinder.GetGridPositions( model.Position, collisionSize, possibleLocations );

			var destinations = possibleLocations.ConvertAll( v =>
			{
				RTS.Path.Pathfinder.DrawBox( v, Color.Blue, 10f );
				return RTS.Path.Pathfinder.GetPosition( v );
			} );

			return destinations;
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

		private void ResetTarget()
		{
			Target = null;
			TargetPosition = null;
			IsGathering = false;
			FollowTarget = false;
			ClearMoveGroup();
		}

		private void ClearMoveGroup()
		{
			if ( MoveGroup != null && MoveGroup.IsValid() )
			{
				MoveGroup.Remove( this );
			}

			MoveGroup = null;
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

			_animationValues.Walking = 0f;

			var isTargetInRange = IsTargetInRange();

			if ( !Target.IsValid() || !isTargetInRange )
			{
				if ( Target.IsValid() && FollowTarget )
				{
					TargetPosition = Target.Position;
				}
				else if ( !IsSelected || !TargetPosition.HasValue )
				{
					if ( Target is ResourceEntity )
						FindTargetResource();
					else if ( Weapon.IsValid() )
						FindTargetUnit();
				}

				var pathDirection = Vector3.Zero;

				if ( MoveGroup != null && MoveGroup.IsValid() )
				{
					if ( MoveGroup.IsDestination( this, Position ) )
					{
						MoveGroup.Finish( this );
					}
					else
					{
						pathDirection = MoveGroup.GetDirection( Position );
					}
				}
				else if ( TargetPosition.HasValue )
				{
					pathDirection = (TargetPosition.Value - Position).Normal;
				}

				if ( pathDirection.Length > 0 )
				{
					_animationValues.Walking = 1f;

					var agents = Physics.GetEntitiesInSphere( Position, _flockSettings.Radius * 5f )
						.Where( entity => entity is UnitEntity unit && IsInMoveGroup( unit ) )
						.Cast<IFlockAgent>();

					CollisionGroup = CollisionGroup.Debris;

					var flocker = new Flocker();
					flocker.Setup( this, agents, Position );
					flocker.Flock( Position + pathDirection.Normal * 50f );

					InputVelocity = (flocker.Force.Normal * Speed).WithZ( 0f );
					Velocity = InputVelocity * Time.Delta;
				}
				else
				{
					Velocity = 0;
				}

				Position += Velocity;
				AlignToGround();

				var worldPos = RTS.Path.Pathfinder.CreateWorldPosition( Position );
				RTS.Path.Pathfinder.DrawBox( worldPos, Color.Green, Time.Delta );

				var walkVelocity = Velocity.WithZ( 0 );

				if ( walkVelocity.Length > 1 )
				{
					Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( walkVelocity.Normal, Vector3.Up ), Time.Delta * 10f );
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

			_animationValues.Lerp( this, "walking", _animationValues.Walking );
		}

		private void AlignToGround()
		{
			Position = Position.WithZ( RTS.Path.Pathfinder.GetHeight( Position ) );
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

