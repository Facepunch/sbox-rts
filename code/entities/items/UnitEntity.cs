using Facepunch.RTS;
using Facepunch.RTS.Units;
using Gamelib.Extensions;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Grid;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public enum UnitTargetType
	{
		None,
		Move,
		Occupy,
		Repair,
		Construct,
		Gather,
		Deposit,
		Attack
	}

	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable, IDamageable, IMoveAgent, IOccupiableEntity
	{
		private class TargetInfo
		{
			public Entity Entity;
			public Vector3? Position;
			public UnitTargetType Type;
			public float Radius;
			public bool Follow;

			public bool HasEntity() => Entity.IsValid();
		}

		private class GatherInfo
		{
			public ResourceEntity Entity;
			public Vector3 Position;
			public ResourceType Type;
			public TimeSince LastGather;
		}

		private struct AnimationValues
		{
			public string Sequence;
			public float Speed;
			public bool Attacking;
			public int HoldType;

			public void Start()
			{
				Speed = 0f;
				HoldType = 0;
				Attacking = false;
			}

			public void Play( AnimEntity entity, string sequence )
			{
				if ( Sequence != sequence )
				{
					entity.PlaybackRate = 1f;
					entity.Sequence = sequence;
					Sequence = sequence;
				}
			}

			public void Finish( AnimEntity entity )
			{
				if ( Speed >= 0.5f )
				{
					Play( entity, "Run_N" );
				}
				else if ( Speed >= 0.5f )
				{
					Play( entity, "Walk_N" );
				}
				else
				{
					Play( entity, "IdlePoseDefault" );
				}

				/*
				entity.SetAnimInt( "holdtype", HoldType );
				entity.SetAnimBool( "attacking", Attacking );
				entity.SetAnimFloat( "speed", entity.GetAnimFloat( "speed" ).LerpTo( Speed, Time.Delta * 10f ) );
				*/
			}
		}

		public override bool HasSelectionGlow => false;

		[Net] public List<UnitEntity> Occupants { get; private set; }
		public bool CanOccupyUnits => Item.Occupiable.Enabled && Occupants.Count < Item.Occupiable.MaxOccupants;
		public UnitTargetType TargetType => _target.Type;
		public IOccupiableItem OccupiableItem => Item;

		public Dictionary<ResourceType, int> Carrying { get; private set; }
		[Net] public Entity Occupiable { get; private set; }
		[Net] public float GatherProgress { get; private set; }
		[Net] public bool IsGathering { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public float LineOfSightRadius { get; private set; }
		[Net, OnChangedCallback] public int Kills { get; set; }
		[Net] public UnitModifiers Modifiers { get; protected set; }
		public override bool CanMultiSelect => true;
		public List<ModelEntity> Clothing => new();
		public UnitCircle Circle { get; private set; }
		public Pathfinder Pathfinder { get; private set; }
		public bool HasBeenSeen { get; set; }
		public float TargetAlpha { get; private set; }
		public float AgentRadius { get; private set; }
		public bool IsStatic { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public MoveGroup MoveGroup { get; private set; }
		public Vector3 InputVelocity { get; private set; }
		public float? SpinSpeed { get; private set; }
		public BaseRank Rank { get; private set; }

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		public EntityHudBar GatherBar { get; private set; }
		public EntityHudIcon RankIcon { get; private set; }
		#endregion

		private readonly GatherInfo _gather = new();
		private readonly TargetInfo _target = new();
		private HashSet<IMoveAgent> _flockAgents { get; set; } = new();
		private List<ISelectable> _targetBuffer = new();
		private RealTimeUntil _nextRepairTime;
		private AnimationValues _animationValues;
		private RealTimeUntil _nextFindTarget;
		private Sound _idleLoopSound;

		public UnitEntity() : base()
		{
			Tags.Add( "unit", "selectable", "ff_ignore" );

			if ( IsServer )
			{
				Carrying = new();
			}

			// Don't collide with anything but static shit.
			CollisionGroup = CollisionGroup.Debris;
			Occupants = new List<UnitEntity>();
			
			// Create the attribute modifiers object.
			CreateModifiers();

			// We start out as a static obstacle.
			IsStatic = true;
		}

		public bool CanConstruct => Item.CanConstruct;

		public void AddKill()
		{
			if ( Host.IsServer )
			{
				Kills += 1;
				UpdateRank( Ranks.Find( Kills ) );
			}
		}

		public Entity GetTargetEntity() => _target.Entity;

		public IList<UnitEntity> GetOccupantsList() => (Occupants as IList<UnitEntity>);

		public void UpdateRank( BaseRank rank )
		{
			if ( Rank == rank ) return;

			Rank?.OnTaken( this );
			Rank = rank;
			Rank.OnGiven( this );
		}

		public bool CanGather( ResourceType type )
		{
			return Item.Gatherables.Contains( type );
		}

		public bool IsTargetValid()
		{
			if ( !_target.HasEntity() ) return false;

			if ( _target.Entity is UnitEntity unit )
			{
				return !unit.Occupiable.IsValid();
			}

			return true;
		}

		public void EvictUnit( UnitEntity unit )
		{
			Host.AssertServer();

			if ( Occupants.Contains( unit ) )
			{
				unit.OnVacate( this );
				Occupants.Remove( unit );
			}
		}

		public void EvictAll()
		{
			for ( int i = 0; i < Occupants.Count; i++ )
			{
				var occupant = Occupants[i];
				occupant.OnVacate( this );
			}

			Occupants.Clear();
		}

		public void GiveHealth( float health )
		{
			Host.AssertServer();

			Health = Math.Min( Health + health, MaxHealth );
		}

		public void MakeStatic( bool isStatic )
		{
			// Don't update if we don't have to.
			if ( IsStatic == isStatic ) return;

			if ( isStatic )
				Tags.Remove( "ff_ignore" );
			else
				Tags.Add( "ff_ignore" );

			Pathfinder.UpdateCollisions( Position, Item.NodeSize * 2f );

			IsStatic = isStatic;
		}

		public Transform? GetAttackAttachment( Entity target )
		{
			var attachments = OccupiableItem.Occupiable.AttackAttachments;
			if ( attachments == null ) return null;

			Transform? closestTransform = null;
			var closestDistance = 0f;
			var targetPosition = target.Position;

			for ( var i = 0; i < attachments.Length; i++ )
			{
				var attachment = GetAttachment( attachments[i], true );
				if ( !attachment.HasValue ) continue;

				var position = attachment.Value.Position;
				var distance = targetPosition.Distance( position );

				if ( !closestTransform.HasValue || distance < closestDistance )
				{
					closestTransform = attachment;
					closestDistance = distance;
				}
			}

			return closestTransform;
		}

		public bool IsTargetInRange()
		{
			if ( !_target.HasEntity() ) return false;

			var target = _target.Entity;
			var radius = _target.Radius;

			if ( Occupiable is IOccupiableEntity occupiable )
			{
				var attackRadius = occupiable.GetAttackRadius();
				return occupiable.IsInRange( target, attackRadius > 0f ? attackRadius : radius );
			}

			var minAttackDistance = Item.MinAttackDistance;

			if ( minAttackDistance > 0f )
			{
				var tolerance = (Pathfinder.NodeSize * 2f);

				if ( target.Position.WithZ( 0f ).Distance( Position.WithZ( 0f ) ) < minAttackDistance - tolerance )
					return false;
			}

			return IsInRange( target, radius );
		}

		public bool IsMoveGroupValid()
		{
			return (MoveGroup != null && MoveGroup.IsValid());
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

			resource.PlayGatherSound();
			resource.Stock -= 1;

			if ( resource.Stock <= 0 )
				resource.Delete();

			return true;
		}

		public override int GetAttackPriority()
		{
			return Item.AttackPriority;
		}

		public override bool CanSelect()
		{
			return !Occupiable.IsValid();
		}

		public override void UpdateHudComponents()
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

			if ( Rank != null )
			{
				RankIcon.SetClass( "hidden", false );
				RankIcon.Texture = Rank.Icon;
			}
			else
			{
				RankIcon.SetClass( "hidden", true );
			}

			base.UpdateHudComponents();
		}

		public override void OnKilled()
		{
			var damageInfo = LastDamageTaken;

			if ( damageInfo.Attacker is UnitEntity unit )
				unit.AddKill();

			if ( Item.RagdollOnDeath )
				BecomeRagdoll( Velocity, damageInfo.Flags, damageInfo.Position, damageInfo.Force, GetHitboxBone( damageInfo.HitboxIndex ) );

			if ( Occupiable.IsValid() && Occupiable is IOccupiableEntity occupiable )
				occupiable.EvictUnit( this );

			CreateDeathParticles();

			LifeState = LifeState.Dead;

			Delete();
		}

		public override void TakeDamage( DamageInfo info )
		{
			info = Resistances.Apply( info, Modifiers.Resistances );

			LastDamageTaken = info;
			DamageOccupants( info );

			base.TakeDamage( info );
		}

		public virtual float GetVerticalSpeed()
		{
			return 20f;
		}

		public virtual float GetVerticalOffset()
		{
			var trace = Trace.Ray( Position.WithZ( 1000f ), Position.WithZ( -1000f ) )
				.WorldOnly()
				.Run();

			return trace.EndPos.z + Item.VerticalOffset;
		}
		
		private Vector3 GetGroundNormal()
		{
			var trace = Trace.Ray( Position.WithZ( 1000f ), Position.WithZ( -1000f ) )
				.WorldOnly()
				.Run();

			var normal = trace.Normal;

			if ( Item.UseBoundsToAlign )
			{
				var normals = new Vector3[5];
				var radius = GetDiameterXY( 0.5f );

				normals[0] = normal;

				var bottomLeft = Position + new Vector3( -radius, -radius );
				var bottomRight = Position + new Vector3( -radius, radius );
				var topRight = Position + new Vector3( radius, -radius );
				var topLeft = Position + new Vector3( radius, radius );

				AddGroundNormal( 1, normals, bottomLeft );
				AddGroundNormal( 2, normals, bottomRight );
				AddGroundNormal( 3, normals, topLeft );
				AddGroundNormal( 4, normals, topRight );

				var averaged = Vector3.Zero;
				var count = normals.Length;

				for ( var i = 0; i < count; i++ )
				{
					averaged += normals[i];
				}

				normal = (averaged / count).Normal;
			}

			return normal;
		}

		private void AddGroundNormal( int index, Vector3[] normals, Vector3 position )
		{
			var trace = Trace.Ray( position.WithZ( 1000f ), position.WithZ( -1000f ) )
			.WorldOnly()
			.Run();

			normals[index] = trace.Normal;
		}

		public virtual bool OccupyUnit( UnitEntity unit )
		{
			Host.AssertServer();

			if ( CanOccupyUnits )
			{
				unit.OnOccupy( this );
				Occupants.Add( unit );
				return true;
			}

			return false;
		}

		public virtual bool CanOccupantsAttack()
		{
			return true;
		}

		public override void StartAbility( BaseAbility ability, AbilityTargetInfo info )
		{
			if ( IsServer ) ClearTarget();

			base.StartAbility( ability, info );
		}

		public override void ClientSpawn()
		{
			Circle = new();
			Circle.Size = GetDiameterXY( 1f, true );
			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			if ( Player.IsValid() && Player.IsLocalPawn )
				Fog.AddViewer( this );
			else
				Fog.AddCullable( this );

			base.ClientSpawn();
		}

		public void DoImpactEffects( Vector3 position, Vector3 normal )
		{
			var impactEffects = Item.ImpactEffects;
			var particleName = impactEffects[Rand.Int( 0, impactEffects.Count - 1 )];

			if ( particleName != null )
			{
				var particles = Particles.Create( particleName, position );
				particles.SetForward( 0, normal );
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

		public Vector3? GetPerimeterPosition( Vector3 target, float radius )
		{
			var pathfinder = Pathfinder;
			var potentialNodes = new List<Vector3>();
			var searchMultiplier = 1.5f;

			pathfinder.GetGridPositions( Position, radius * searchMultiplier, potentialNodes, true );

			var freeLocations = potentialNodes
				.Where( v => v.Distance( target ) >= radius )
				.OrderBy( v => v.Distance( Position ) )
				.ToList();

			if ( freeLocations.Count > 0 )
				return freeLocations[0];
			else
				return null;
		}

		public void Attack( ISelectable target, bool autoFollow = true, MoveGroup moveGroup = null )
		{
			Attack( (ModelEntity)target, autoFollow, moveGroup );
		}

		public void Attack( ModelEntity target, bool autoFollow = true, MoveGroup moveGroup = null )
		{
			ResetTarget();

			_target.Entity = target;
			_target.Follow = autoFollow;
			_target.Radius = Item.AttackRadius;
			_target.Type = UnitTargetType.Attack;

			SetMoveGroup( moveGroup );
			OnTargetChanged();
		}

		public void MoveTo( MoveGroup group )
		{
			ResetTarget();

			_target.Type = UnitTargetType.Move;

			SetMoveGroup( group );
			OnTargetChanged();
		}

		public void MoveTo( Vector3 position )
		{
			ResetTarget();

			_target.Type = UnitTargetType.Move;

			SetMoveGroup( CreateMoveGroup( position ) );
			OnTargetChanged();
		}

		public MoveGroup CreateMoveGroup( Vector3 destination )
		{
			var moveGroup = new MoveGroup();
			moveGroup.Initialize( this, destination );
			return moveGroup;
		}

		public MoveGroup CreateMoveGroup( List<Vector3> destinations )
		{
			var moveGroup = new MoveGroup();
			moveGroup.Initialize( this, destinations );
			return moveGroup;
		}

		public bool CanOccupy( IOccupiableEntity occupiable )
		{
			var whitelist = occupiable.OccupiableItem.Occupiable.Whitelist;

			if ( whitelist.Count == 0 )
				return true;

			return whitelist.Contains( Item.UniqueId );
		}

		public bool Occupy( IOccupiableEntity occupiable, MoveGroup moveGroup = null )
		{
			var modelEntity = (occupiable as ModelEntity);

			if ( modelEntity == null )
			{
				ClearTarget();
				return false;
			}

			moveGroup ??= CreateMoveGroup( GetDestinations( modelEntity ) );

			if ( !moveGroup.IsValid() )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = modelEntity;
			_target.Radius = Item.InteractRadius + (Pathfinder.CollisionSize * 2);
			_target.Type = UnitTargetType.Occupy;

			SetMoveGroup( moveGroup );
			OnTargetChanged();

			return true;
		}

		public bool Deposit( BuildingEntity building, MoveGroup moveGroup = null )
		{
			moveGroup ??= CreateMoveGroup( GetDestinations( building ) );

			if ( !moveGroup.IsValid() )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = building;
			_target.Radius = Item.InteractRadius + (Pathfinder.CollisionSize * 2);
			_target.Type = UnitTargetType.Deposit;

			SetMoveGroup( moveGroup );

			OnTargetChanged();

			return true;
		}

		public bool Gather( ResourceEntity resource, MoveGroup moveGroup = null )
		{
			moveGroup ??= CreateMoveGroup( GetDestinations( resource ) );

			if ( !moveGroup.IsValid() )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = resource;
			_target.Radius = Item.InteractRadius + (Pathfinder.CollisionSize * 2);
			_target.Type = UnitTargetType.Gather;

			if ( _gather.Entity.IsValid() )
				_gather.Entity.RemoveGatherer( this );

			_gather.Type = resource.Resource;
			_gather.Entity = resource;
			_gather.Entity.AddGatherer( this );
			_gather.Position = resource.Position;

			SetMoveGroup( moveGroup );
			OnTargetChanged();

			return true;
		}

		public bool Repair( BuildingEntity building, MoveGroup moveGroup = null )
		{
			moveGroup ??= CreateMoveGroup( GetDestinations( building ) );

			if ( !moveGroup.IsValid() )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = building;
			_target.Radius = Item.InteractRadius + (Pathfinder.CollisionSize * 2);
			_target.Type = UnitTargetType.Repair;

			SetMoveGroup( moveGroup );
			OnTargetChanged();

			return true;
		}

		public bool Construct( BuildingEntity building, MoveGroup moveGroup = null )
		{
			moveGroup ??= CreateMoveGroup( GetDestinations( building ) );

			if ( !moveGroup.IsValid() )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = building;
			_target.Radius = Item.InteractRadius + (Pathfinder.CollisionSize * 2);
			_target.Type = UnitTargetType.Construct;

			SetMoveGroup( moveGroup );
			OnTargetChanged();

			return true;
		}

		public void ClearTarget()
		{
			_target.Entity = null;
			_target.Position = null;
			_target.Follow = false;
			_target.Type = UnitTargetType.None;

			IsGathering = false;

			ClearMoveGroup();
			OnTargetChanged();
		}

		public float GetAttackRadius() => Item.AttackRadius;
		public float GetMinVerticalRange() => Item.MinVerticalRange;
		public float GetMaxVerticalRange() => Item.MaxVerticalRange;

		public float LookAtPosition( Vector3 position, float? interpolation = null )
		{
			var targetDirection = (position - Position).WithZ( 0f );
			var targetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

			if ( interpolation.HasValue )
				Rotation = Rotation.Lerp( Rotation, targetRotation, interpolation.Value );
			else
				Rotation = targetRotation;

			return Rotation.Distance( targetRotation );
		}

		public float LookAtEntity( Entity target, float? interpolation = null )
		{
			return LookAtPosition( target.Position, interpolation );
		}

		public void MakeVisible( bool isVisible )
		{
			TargetAlpha = isVisible ? 1f : 0f;
			Hud.SetActive( isVisible );
		}

		public ModelEntity AttachClothing( string modelName )
		{
			var entity = new Clothes();

			entity.SetModel( modelName );
			entity.SetParent( this, true );

			Clothing.Add( entity );

			return entity;
		}

		public void OnMoveGroupDisposed()
		{
			_target.Position = null;
			_target.Follow = false;

			IsGathering = false;

			SetMoveGroup( null );
			OnTargetChanged();
		}

		public List<Vector3> GetDestinations( ModelEntity model )
		{
			var pathfinder = Pathfinder;
			var collisionSize = pathfinder.CollisionSize;
			var nodeSize = pathfinder.NodeSize;

			// Round up the radius to the nearest node size.
			var radius = MathF.Ceiling( (model.GetDiameterXY( 0.5f ) + collisionSize / 2f) / nodeSize ) * nodeSize;
			var possibleLocations = new List<Vector3>();

			pathfinder.GetGridPositions( model.Position, radius, possibleLocations, true );

			return possibleLocations;
		}

		public bool InVerticalRange( ISelectable other )
		{
			var selfPosition = Position;
			var minVerticalRange = Item.MinVerticalRange;
			var maxVerticalRange = Item.MaxVerticalRange;

			if ( Occupiable is IOccupiableEntity occupiable )
			{
				selfPosition = occupiable.Position;
				minVerticalRange = occupiable.GetMinVerticalRange();
				maxVerticalRange = occupiable.GetMaxVerticalRange();
			}

			var distance = Math.Abs(selfPosition.z - other.Position.z);
			return (distance >= minVerticalRange && distance <= maxVerticalRange);
		}

		public float GetSpeed()
		{
			return Item.Speed * Modifiers.Speed;
		}

		public void RemoveClothing()
		{
			Clothing.ForEach( ( entity ) => entity.Delete() );
			Clothing.Clear();
		}

		public Vector3? GetVacatePosition( UnitEntity unit )
		{
			return GetFreePosition( unit, 1.5f );
		}

		public virtual void OnOccupy( IOccupiableEntity occupiable )
		{
			Deselect();
			SetParent( this );

			Occupiable = (Entity)occupiable;
			EnableAllCollisions = false;
			EnableDrawing = false;
		}

		public virtual void OnVacate( IOccupiableEntity occupiable )
		{
			SetParent( null );

			var position = occupiable.GetVacatePosition( this );

			if ( position.HasValue )
			{
				Position = position.Value;
				ResetInterpolation();
			}

			Rotation = Rotation.Identity;
			Occupiable = null;
			EnableAllCollisions = true;
			EnableDrawing = true;
		}

		public virtual void DamageOccupants( DamageInfo info )
		{
			var scale = Item.Occupiable.DamageScale;
			if ( scale <= 0f ) return;

			var occupants = Occupants;
			var occupantsCount = occupants.Count;
			if ( occupantsCount == 0 ) return;

			info.Damage *= scale;

			for ( var i = occupantsCount - 1; i >= 0; i-- )
			{
				var occupant = occupants[i];
				occupant.TakeDamage( info );
			}
		}

		protected override void OnPlayerAssigned( Player player )
		{
			if ( Item.UseRenderColor )
				RenderColor = Player.TeamColor;
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				Circle?.Delete();
				Fog.RemoveViewer( this );
				Fog.RemoveCullable( this );
			}
			else
			{
				if ( Player.IsValid() )
					Player.TakePopulation( Item.Population );

				_idleLoopSound.Stop();

				ClearMoveGroup();
			}

			base.OnDestroy();
		}

		protected override void ServerTick()
		{
			base.ServerTick();

			Velocity = 0;

			_animationValues.Start();

			if ( !Occupiable.IsValid() )
			{
				if ( !IsUsingAbility() )
				{
					var isTargetInRange = IsTargetInRange();
					var isTargetValid = IsTargetValid();

					if ( _target.Type == UnitTargetType.Attack )
					{
						if ( !isTargetValid )
							ClearTarget();
						else if ( !IsMoveGroupValid() )
							ValidateAttackDistance();
					}

					if ( isTargetValid && isTargetInRange )
					{
						ClearMoveGroup();
						TickInteractWithTarget();
					}
					else
					{
						TickMoveToTarget( isTargetValid );
					}

					TickFindTarget();
				}
				else
				{
					TickAbility();
				}
			}
			else if ( Item.Occupant?.CanAttack == true )
			{
				TickOccupantAttack();
			}

			if ( Weapon.IsValid() )
			{
				_animationValues.Attacking = Weapon.LastAttack < 0.1f;
				_animationValues.HoldType = Weapon.HoldType;
			}

			_animationValues.Finish( this );
		}

		protected override void OnItemChanged( BaseUnit item, BaseUnit oldItem )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );

				var materialGroups = MaterialGroupCount;

				if ( materialGroups > 0 )
					SetMaterialGroup( Rand.Int( 0, materialGroups ) );
			}

			RemoveClothing();

			foreach ( var clothes in item.Clothing )
			{
				AttachClothing( clothes );
			}

			Health = item.MaxHealth;
			MaxHealth = item.MaxHealth;
			EyePos = Position + Vector3.Up * 64;
			LineOfSightRadius = item.LineOfSightRadius;
			CollisionGroup = CollisionGroup.Player;
			EnableHitboxes = true;

			if ( oldItem  != null )
			{
				// Remove the old base resistances.
				foreach ( var kv in item.Resistances )
					Modifiers.AddResistance( kv.Key, -kv.Value );
			}

			// Add the new base resistances.
			foreach ( var kv in item.Resistances )
				Modifiers.AddResistance( kv.Key, kv.Value );

			if ( item.UsePathfinder )
				Pathfinder = PathManager.GetPathfinder( item.NodeSize, item.CollisionSize );
			else
				Pathfinder = PathManager.Default;

			if ( item.UseModelPhysics )
				SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			else
				SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, item.NodeSize * 0.5f ) );

			LocalCenter = CollisionBounds.Center;
			AgentRadius = GetDiameterXY( Item.AgentRadiusScale );

			_idleLoopSound.Stop();

			if ( !string.IsNullOrEmpty( item.IdleLoopSound ) )
				_idleLoopSound = PlaySound( item.IdleLoopSound );

			if ( Weapon.IsValid() ) Weapon.Delete();

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
					Weapon.SetParent( this, Weapon.BoneMerge );
				}
			}

			Position = Position.WithZ( GetVerticalOffset() );

			base.OnItemChanged( item, oldItem );
		}

		protected virtual void CreateModifiers()
		{
			Modifiers = new UnitModifiers();
		}

		// TODO: I don't want to do half of this shit each tick.
		protected override void ClientTick()
		{
			base.ClientTick();

			if ( Hud.Style.Opacity != RenderAlpha )
			{
				Hud.Style.Opacity = RenderAlpha;
				Hud.Style.Dirty();
			}

			Hud.SetActive( RenderAlpha > 0f );

			if ( Occupiable.IsValid() )
			{
				Circle.Alpha = 0f;
				RenderAlpha = 0f;

				return;
			}

			if ( Circle.IsValid() && Player.IsValid() )
			{
				if ( IsLocalPlayers && IsSelected )
					Circle.Color = Color.White;
				else
					Circle.Color = Player.TeamColor;

				Circle.Alpha = 1f;
			}

			if ( IsLocalPlayers )
			{
				var isOnScreen = IsOnScreen();

				Circle.Alpha = isOnScreen ? 1f : 0f;
				RenderAlpha = isOnScreen ? 1f : 0f;
				
				return;
			}

			RenderAlpha = RenderAlpha.LerpTo( TargetAlpha, Time.Delta * 2f );

			for ( var i = 0; i < Children.Count; i++ )
			{
				if ( Children[i] is ModelEntity child )
				{
					child.RenderAlpha = RenderAlpha;
				}
			}

			if ( Circle.IsValid() )
			{
				Circle.Alpha = RenderAlpha;
			}
		}

		[ClientRpc]
		protected virtual void CreateDeathParticles()
		{
			if ( !string.IsNullOrEmpty( Item.DeathParticles ) )
			{
				var particles = Particles.Create( Item.DeathParticles );
				particles.SetPosition( 0, Position );
			}
		}

		protected virtual void OnTargetChanged()
		{
			if ( Weapon.IsValid() )
			{
				Weapon.Target = _target.Entity;
				Weapon.Occupiable = Occupiable;
			}

			SpinSpeed = null;
		}

		protected override void AddHudComponents()
		{
			RankIcon = Hud.AddChild<EntityHudIcon>( "rank" );
			HealthBar = Hud.AddChild<EntityHudBar>( "health" );

			if ( IsLocalPlayers )
			{
				GatherBar = Hud.AddChild<EntityHudBar>( "gather" );
			}

			base.AddHudComponents();
		}

		[ClientRpc]
		private void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
		{
			Ragdoll.From( this, velocity, damageFlags, forcePos, force, bone ).FadeOut( 10f );
		}

		private void OnKillsChanged()
		{
			UpdateRank( Ranks.Find( Kills ) );
		}

		private void SetMoveGroup( MoveGroup group )
		{
			MoveGroup = group;
		}

		private void ResetTarget()
		{
			_target.Entity = null;
			_target.Position = null;
			_target.Follow = false;
			_target.Type = UnitTargetType.None;

			IsGathering = false;

			ClearMoveGroup();
		}

		private void ClearMoveGroup()
		{
			if ( MoveGroup != null && MoveGroup.IsValid() )
			{
				MoveGroup.Remove( this );
			}

			SetMoveGroup( null );
		}

		private void FindTargetResource()
		{
			// If our last resource entity is valid just use that.
			if ( _gather.Entity.IsValid() )
			{
				Gather( _gather.Entity );
				return;
			}

			var entities = Physics.GetEntitiesInSphere( _gather.Position, 1000f );

			foreach ( var entity in entities )
			{
				if ( entity is ResourceEntity resource && resource.Resource == _gather.Type )
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

		private void FindTargetEnemy()
		{
			var searchPosition = Position;
			var searchRadius = Item.AttackRadius;

			if ( Occupiable is IOccupiableEntity occupiable )
			{
				var attackRadius = occupiable.GetAttackRadius();

				if ( attackRadius > 0f )
					searchRadius = attackRadius;

				searchPosition = occupiable.Position;
			}

			var entities = Physics.GetEntitiesInSphere( searchPosition.WithZ( 0f ), searchRadius * 1.2f );

			_targetBuffer.Clear();

			foreach ( var entity in entities )
			{
				if ( entity is ISelectable selectable )
				{
					if ( IsEnemy( selectable ) && InVerticalRange( selectable ) )
						_targetBuffer.Add( selectable );
				}
			}

			_targetBuffer.OrderByDescending( s => s.GetAttackPriority() ).ThenBy( s => s.Position.Distance( searchPosition ) );

			if ( _targetBuffer.Count > 0 )
			{
				Attack( _targetBuffer[0], false );
			}
		}

		private void TickFindTarget()
		{
			if ( IsSelected || IsMoveGroupValid() || !Weapon.IsValid() )
				return;

			if ( _target.Follow )
				return;

			if ( _nextFindTarget )
			{
				FindTargetEnemy();
				_nextFindTarget = 1;
			}
		}

		private void TickOccupantAttack()
		{
			if ( Occupiable is not IOccupiableEntity occupiable )
				return;

			if ( occupiable.CanOccupantsAttack() && IsTargetValid() )
			{
				if ( IsTargetInRange() && _target.Type == UnitTargetType.Attack )
				{
					if ( Weapon.IsValid() && Weapon.CanAttack() )
					{
						Weapon.Attack();
					}
				}
			}

			TickFindTarget();
		}

		private void TickInteractWithTarget()
		{
			var lookAtDistance = 0f;

			if ( !SpinSpeed.HasValue )
				lookAtDistance = LookAtEntity( _target.Entity, Time.Delta * Item.RotateToTargetSpeed );
			else
				Rotation = Rotation.FromYaw( Rotation.Yaw() + SpinSpeed.Value * Time.Delta );

			if ( SpinSpeed.HasValue || lookAtDistance.AlmostEqual( 0f, 0.1f ) )
			{
				if ( _target.Type == UnitTargetType.Occupy )
				{
					if ( _target.Entity is IOccupiableEntity occupiable && occupiable.Player == Player )
					{
						if ( occupiable.CanOccupyUnits )
						{
							TickOccupy( occupiable );
							return;
						}
					}
				}

				if ( _target.Type == UnitTargetType.Construct )
				{
					if ( _target.Entity is BuildingEntity building && building.Player == Player )
					{
						if ( building.IsUnderConstruction )
						{
							TickConstruct( building );
							return;
						}
					}
				}

				if ( _target.Type == UnitTargetType.Repair )
				{
					if ( _target.Entity is BuildingEntity building && building.Player == Player )
					{
						if ( !building.IsUnderConstruction && building.IsDamaged() )
						{
							TickRepair( building );
							return;
						}
					}
				}

				if ( _target.Type == UnitTargetType.Deposit )
				{
					if ( _target.Entity is BuildingEntity building && building.Player == Player )
					{
						if ( building.CanDepositResources )
						{
							DepositResources();
							return;
						}
					}
				}

				if ( _target.Type == UnitTargetType.Gather )
				{
					if ( _target.Entity is ResourceEntity resource )
					{
						if ( SpinSpeed.HasValue || lookAtDistance.AlmostEqual( 0f, 0.1f ) )
						{
							TickGather( resource );
							return;
						}
					}
				}

				if ( _target.Type == UnitTargetType.Attack )
				{
					if ( Weapon.IsValid() && Weapon.CanAttack() )
					{
						if ( lookAtDistance.AlmostEqual( 0f, Weapon.RotationTolerance ) )
						{
							Weapon.Attack();
							return;
						}
					}
				}
			}
		}

		private bool ValidateAttackDistance()
		{
			if ( !_target.HasEntity() ) return false;

			var minAttackDistance = Item.MinAttackDistance;
			var target = _target.Entity;

			if ( minAttackDistance == 0f ) return true;

			var tolerance = (Pathfinder.NodeSize * 2f);
			var targetPosition = target.Position.WithZ( 0f );
			var selfPosition = Position.WithZ( 0f );

			if ( targetPosition.Distance( selfPosition ) >= minAttackDistance - tolerance )
				return true;

			var position = GetPerimeterPosition( targetPosition, minAttackDistance );

			if ( !position.HasValue )
				return true;

			SetMoveGroup( CreateMoveGroup( position.Value ) );

			return false;
		}

		private void TickAbility()
		{
			var ability = UsingAbility;

			if ( ability == null || !ability.LookAtTarget )
				return;

			LookAtPosition( ability.TargetInfo.Origin, Time.Delta * Item.RotateToTargetSpeed );
		}

		private bool IsAtDestination()
		{
			if ( !IsMoveGroupValid() )
				return true;

			if ( Item.UsePathfinder )
				return MoveGroup.IsDestination( this, Position );

			var groundPosition = Position.WithZ( 0f );
			var destination = MoveGroup.GetDestination();

			if ( groundPosition.Distance( destination ) <= AgentRadius * 0.5f )
				return true;

			return MoveGroup.IsDestination( this, Position, false );
		}

		private void TickMoveToTarget( bool isTargetValid )
		{
			if ( isTargetValid && _target.Follow )
			{
				_target.Position = _target.Entity.Position;
			}

			var steerDirection = Vector3.Zero;
			var pathDirection = Vector3.Zero;
			var movementSpeed = GetSpeed();

			if ( IsMoveGroupValid() )
			{
				var node = Pathfinder.CreateWorldPosition( Position );
				Pathfinder.DrawBox( node, Color.Green );

				if ( IsAtDestination() )
				{
					MoveGroup.Finish( this );
				}
				else
				{
					Vector3 direction;

					if ( Item.UsePathfinder )
					{
						var offset = Pathfinder.CenterOffset.Normal;
						direction = MoveGroup.GetDirection( Position );
						pathDirection = (direction.Normal * offset).WithZ( 0f );
					}
					else
					{
						direction = (MoveGroup.GetDestination() - Position).Normal;
						pathDirection = direction.WithZ( 0f );
					}

					_flockAgents.Clear();
					_flockAgents.UnionWith( MoveGroup.Agents );

					if ( _gather.Entity.IsValid() )
					{
						_flockAgents.UnionWith( _gather.Entity.Gatherers );
					}

					if ( _flockAgents.Count > 1 )
					{
						var flocker = new Flocker();
						flocker.Setup( this, _flockAgents, Position );
						flocker.Flock( Position + direction * Pathfinder.NodeSize );
						steerDirection = flocker.Force.Normal.WithZ( 0f );
					}
				}
			}
			else if ( _target.Position.HasValue )
			{
				var straightDirection = (_target.Position.Value - Position).Normal.WithZ( 0f );

				if ( Pathfinder.IsAvailable( Position + (straightDirection * Pathfinder.NodeSize) ) )
					pathDirection = straightDirection;
				else
					pathDirection = Vector3.Zero;
			}
			else if ( !IsSelected )
			{
				if ( _target.Entity is ResourceEntity )
					FindTargetResource();
			}

			if ( pathDirection.Length > 0 )
			{
				if ( movementSpeed >= 300f )
					_animationValues.Speed = 1f;
				else
					_animationValues.Speed = 0.5f;

				// First we'll try our steer direction and see if we can go there.
				if ( steerDirection.Length > 0 )
				{
					if ( !Item.UsePathfinder || Pathfinder.IsAvailable( Position + (steerDirection * Pathfinder.NodeSize) ) )
					{
						Velocity = (steerDirection * movementSpeed) * Time.Delta;
					}
				}

				if ( Velocity.Length == 0 )
				{
					Velocity = (pathDirection * movementSpeed) * Time.Delta;
				}
			}
			else
			{
				Velocity = 0;
			}

			var verticalOffset = GetVerticalOffset();

			Position += Velocity;
			Position = Position.LerpTo( Position.WithZ( verticalOffset ), Time.Delta * GetVerticalSpeed() );

			if ( Item.AlignToSurface )
			{
				var normal = GetGroundNormal();
				var targetRotation = Rotation.LookAt( normal, Rotation.Forward );
				targetRotation = targetRotation.RotateAroundAxis( Vector3.Left, 90f );
				targetRotation = targetRotation.RotateAroundAxis( Vector3.Up, 180f );
				Rotation = Rotation.Lerp( Rotation, targetRotation, Time.Delta * movementSpeed / 20f );
			}

			var walkVelocity = Velocity.WithZ( 0 );

			if ( walkVelocity.Length > 1 )
			{
				Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( walkVelocity.Normal, Vector3.Up ), Time.Delta * 10f );
			}
		}

		private void TickOccupy( IOccupiableEntity occupiable )
		{
			if ( occupiable.OccupyUnit( this ) )
				ClearTarget();
		}

		private void DepositResources()
		{
			ResourceHint.Send( Player, 2f, Position, Carrying, Color.Green );

			foreach ( var kv in Carrying )
			{
				Player.GiveResource( kv.Key, kv.Value );
			}

			Carrying.Clear();

			FindTargetResource();
		}

		private void TickRepair( BuildingEntity building )
		{
			var repairAmount = building.MaxHealth / building.Item.BuildTime * 0.5f;
			var fraction = repairAmount / building.MaxHealth;
			var repairCosts = new Dictionary<ResourceType, int>();

			foreach ( var kv in building.Item.Costs )
			{
				repairCosts[kv.Key] = (kv.Value * fraction).CeilToInt();
			}

			if ( !Player.CanAfford( repairCosts ) )
			{
				LookAtEntity( building );
				SpinSpeed = 0f;
				return;
			}

			SpinSpeed = (building.MaxHealth / building.Health) * 200f;

			if ( !_nextRepairTime ) return;

			Player.TakeResources( repairCosts );

			ResourceHint.Send( Player, 0.5f, Position, repairCosts, Color.Red );

			building.Health += repairAmount;
			building.Health = building.Health.Clamp( 0f, building.Item.MaxHealth );

			if ( building.Health == building.Item.MaxHealth )
			{
				LookAtEntity( building );
				building.FinishRepair();
				ClearTarget();
			}
			else
			{
				building.UpdateRepair();
			}

			_nextRepairTime = 1f;
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
			if ( _gather.LastGather < resource.GatherTime )
				return;

			TakeFrom( resource );

			_gather.LastGather = 0;
			IsGathering = true;

			if ( !Carrying.TryGetValue( resource.Resource, out var carrying ) )
				return;

			GatherProgress = (1f / resource.MaxCarry) * carrying;

			if ( carrying < resource.MaxCarry ) return;

			// We're full, let's deposit that shit.
			FindResourceDepo();
		}
	}
}

