using Facepunch.RTS;
using Facepunch.RTS.Commands;
using Facepunch.RTS.Units;
using Facepunch.RTS.Upgrades;
using Gamelib.Extensions;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Extensions;
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

	public partial class UnitEntity : ItemEntity<BaseUnit>, IFogViewer, IFogCullable, IDamageable, IMoveAgent, IOccupiableEntity, IMapIconEntity
	{
		protected class TargetInfo
		{
			public Entity Entity;
			public Vector3? Position;
			public UnitTargetType Type;
			public float Radius;
			public bool Follow;

			public bool HasEntity() => Entity.IsValid();
		}

		protected class GatherInfo
		{
			public ResourceEntity Entity;
			public Vector3 Position;
			public ResourceType Type;
			public TimeSince LastGather;
		}

		public override bool HasSelectionGlow => false;

		public Dictionary<string, float> Resistances { get; set; }
		[Net, OnChangedCallback] private List<float> ResistanceList { get; set; }
		[Net] public List<UnitEntity> Occupants { get; private set; }
		public bool CanOccupyUnits => Item.Occupiable.Enabled && Occupants.Count < Item.Occupiable.MaxOccupants;
		public IOccupiableItem OccupiableItem => Item;

		public Dictionary<ResourceType, int> Carrying { get; private set; }
		[Net] public UnitTargetType TargetType { get; protected set; }
		[Net] public Entity Occupiable { get; private set; }
		[Net] public float GatherProgress { get; private set; }
		[Net] public bool IsGathering { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public float LineOfSightRadius { get; private set; }
		[Net] public Vector3 Destination { get; private set; }
		[Net] public TimeSince LastDamageTime { get; private set; }
		[Net, OnChangedCallback] public int Kills { get; set; }
		[Net] public UnitModifiers Modifiers { get; protected set; }
		public override bool CanMultiSelect => true;
		public List<ModelEntity> Clothing => new();
		public UnitCircle Circle { get; private set; }
		public Pathfinder Pathfinder { get; private set; }
		public Color IconColor => Player.TeamColor;
		public bool HasBeenSeen { get; set; }
		public float TargetAlpha { get; private set; }
		public float AgentRadius { get; private set; }
		public bool IsStatic { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public Stack<MoveGroup> MoveStack { get; private set; }
		public Vector3 TargetVelocity { get; private set; }
		public float? SpinSpeed { get; private set; }
		public bool IsVisible { get; set; }
		public BaseRank Rank { get; private set; }

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		public EntityHudBar GatherBar { get; private set; }
		public EntityHudIcon RankIcon { get; private set; }
		#endregion

		protected readonly GatherInfo _gather = new();
		protected readonly TargetInfo _target = new();
		protected IMoveAgent[] _flockBuffer = new IMoveAgent[8];
		protected List<ISelectable> _targetBuffer = new();
		protected Particles _pathParticles;
		protected RealTimeUntil _nextRepairTime;
		protected RealTimeUntil _nextFindTarget;
		protected Sound _idleLoopSound;

		public UnitEntity() : base()
		{
			Tags.Add( "unit", "selectable", "ff_ignore" );

			if ( IsServer )
			{
				Carrying = new();
			}

			ResistanceList = new List<float>();
			Resistances = new();

			// Don't collide with anything but static shit.
			EnableDrawOverWorld = true;
			CollisionGroup = CollisionGroup.Debris;
			Occupants = new List<UnitEntity>();
			MoveStack = new();

			// Create the attribute modifiers object.
			CreateModifiers();

			// We start out as a static obstacle.
			IsStatic = true;
		}

		public bool CanConstruct => Item.CanConstruct;

		public MoveGroup MoveGroup
		{
			get
			{
				if ( MoveStack.TryPeek( out var group ) )
					return group;
				else
					return null;
			}
		}

		public void AddKill()
		{
			if ( Host.IsServer )
			{
				Kills += 1;
				UpdateRank( Ranks.Find( Kills ) );
			}
		}

		public bool CanAttackTarget( IDamageable target )
		{
			if ( target is ISelectable selectable )
				return CanAttackTarget( selectable );

			var entity = target as Entity;

			if ( !InVerticalRange( entity ) )
				return false;

			if ( Weapon.IsValid() )
			{
				if ( Weapon.TargetTeam == WeaponTargetTeam.Ally )
					return false;
			}

			return target.CanBeAttacked();
		}
		
		public bool CanAttackTarget( ISelectable target )
		{
			if ( target == this )
				return false;

			if ( !target.CanBeAttacked() )
				return false;

			if ( !InVerticalRange( target ) )
				return false;

			if ( !Weapon.IsValid() )
				return false;

			if ( Weapon.TargetType == WeaponTargetType.Building && target is not BuildingEntity )
				return false;

			if ( Weapon.TargetType == WeaponTargetType.Unit && target is not UnitEntity )
				return false;

			if ( !Weapon.CanTarget( target ) )
				return false;

			if ( Weapon.TargetTeam == WeaponTargetTeam.Ally )
				return !IsEnemy( target );
			else
				return IsEnemy( target );
		}

		public bool IsAtDestination()
		{
			if ( !MoveStack.TryPeek( out var group ) || !group.IsValid() )
				return true;

			if ( Item.UsePathfinder )
				return group.IsDestination( this, Position );

			var groundPosition = Position.WithZ( 0f );
			var groundDestination = group.GetDestination().WithZ( 0f );
			var tolerance = AgentRadius * 0.1f;

			if ( groundPosition.Distance( groundDestination ) <= tolerance )
				return true;

			return group.IsDestination( this, Position, false );
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

		public bool CanGatherAny()
		{
			return Item.Gatherables.Count > 0;
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

				if ( attackRadius == 0f )
					attackRadius = radius;

				return occupiable.IsInRange( target, attackRadius );
			}

			if ( _target.Type == UnitTargetType.Attack )
			{
				var minAttackDistance = Item.MinAttackDistance;

				if ( minAttackDistance > 0f )
				{
					var tolerance = (Pathfinder.NodeSize * 2f);

					if ( target.Position.WithZ( 0f ).Distance( Position.WithZ( 0f ) ) < minAttackDistance - tolerance )
						return false;
				}

				return IsInRange( target, radius );
			}
			else
			{
				return IsInRange( target, radius, 1.5f );
			}
		}

		public void Kill( DamageInfo damageInfo = default )
		{
			if ( Item.RagdollOnDeath )
				BecomeRagdoll( Velocity, damageInfo.Flags, damageInfo.Position, damageInfo.Force, GetHitboxBone( damageInfo.HitboxIndex ) );

			CreateDeathParticles();
			LifeState = LifeState.Dead;
			Delete();
		}

		public bool IsMoveGroupValid()
		{
			if ( MoveStack.TryPeek( out var group ) )
				return group.IsValid();
			else
				return false;
		}

		public void AddResistance( string id, float amount )
		{
			Host.AssertServer();

			var resistance = RTS.Resistances.Find( id );

			if ( Resistances.ContainsKey( id ) )
				Resistances[id] += amount;
			else
				Resistances[id] = amount;

			Resistances[id] = Resistances[id].Clamp( -1f, 1f );

			var networkId = resistance.NetworkId;

			while ( ResistanceList.Count <= networkId )
			{
				ResistanceList.Add( 0f );
			}

			ResistanceList[(int)networkId] = Resistances[id];
		}

		public bool TakeFrom( ResourceEntity resource )
		{
			if ( resource.Stock <= 0 ) return false;

			if ( Carrying.TryGetValue( resource.Resource, out var carrying ) )
			{
				var maxCarry = resource.MaxCarry * Item.MaxCarryMultiplier;

				if ( carrying < maxCarry )
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

			Kill( damageInfo );
		}

		public override void TakeDamage( DamageInfo info )
		{
			info = RTS.Resistances.Apply( info, Resistances );

			LastDamageTaken = info;
			LastDamageTime = 0;

			DamageOccupants( info );

			base.TakeDamage( info );
		}

		public virtual bool ShouldShowOnMap()
		{
			return IsLocalTeamGroup || IsVisible;
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

		private void OnResistanceListChanged()
		{
			for ( var i = 0; i < ResistanceList.Count; i++ )
			{
				var resistance = RTS.Resistances.Find<BaseResistance>( (uint)i );
				var uniqueId = resistance.UniqueId;

				if ( ResistanceList[i] != 0f )
					Resistances[uniqueId] = ResistanceList[i];
				else if ( Resistances.ContainsKey( uniqueId ) )
					Resistances.Remove( uniqueId );
			}
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

		public override void ClientSpawn()
		{
			Circle = new();

			if ( Item != null )
				Circle.Size = GetDiameterXY( Item.CircleScale, true );
			else
				Circle.Size = GetDiameterXY( 1f, true );

			Circle.SetParent( this );
			Circle.LocalPosition = Vector3.Zero;

			if ( IsLocalTeamGroup )
				Fog.AddViewer( this );
			else
				Fog.AddCullable( this );

			MiniMap.Instance.AddEntity( this, "unit" );

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
			/*
			var damageDecals = Item.DamageDecals;

			if ( damageDecals.Count == 0 ) return;

			var randomDecalName = damageDecals[Rand.Int( 0, damageDecals.Count - 1 )];
			var decalMaterial = Material.Load( randomDecalName );
			var decalRotation = Rotation.LookAt( Vector3.Up ) * Rotation.FromAxis( Vector3.Forward, Rand.Float( 0f, 360f ) );
			var randomSize = Rand.Float( 50f, 100f );
			var trace = Trace.Ray( position, position + Vector3.Down * 100f ).Ignore( this ).Run();

			Decals.Place( decalMaterial, trace.Entity, trace.Bone, trace.EndPos, new Vector3( randomSize, randomSize, 4f ), decalRotation );
			*/
		}

		public bool IsInMoveGroup( UnitEntity other )
		{
			if ( MoveStack.TryPeek( out var a ) && other.MoveStack.TryPeek( out var b ) )
				return (a == b);
			else
				return false;
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

		public void SetAttackTarget( IDamageable target, bool autoFollow = true )
		{
			SetAttackTarget( (ModelEntity)target, autoFollow );
		}

		public void SetAttackTarget( ISelectable target, bool autoFollow = true )
		{
			SetAttackTarget( (ModelEntity)target, autoFollow );
		}

		public void SetAttackTarget( ModelEntity target, bool autoFollow = true )
		{
			ResetTarget();

			_target.Entity = target;
			_target.Follow = autoFollow;
			_target.Radius = Item.AttackRadius;
			_target.Type = UnitTargetType.Attack;

			OnTargetChanged();
		}

		public void SetMoveTarget( MoveGroup group )
		{
			ResetTarget();

			_target.Type = UnitTargetType.Move;

			OnTargetChanged();
		}

		public void SetMoveTarget( Vector3 position )
		{
			ResetTarget();

			_target.Type = UnitTargetType.Move;

			OnTargetChanged();
		}

		public MoveGroup PushMoveGroup( MoveGroup group )
		{
			TryFinishMoveGroup();
			MoveStack.Push( group );
			return group;
		}

		public MoveGroup PushMoveGroup( IMoveCommand command )
		{
			TryFinishMoveGroup();

			var moveGroup = new MoveGroup();
			moveGroup.Initialize( this, command );

			MoveStack.Push( moveGroup );

			return moveGroup;
		}

		public MoveGroup PushMoveGroup( Vector3 destination, IMoveCommand command = null )
		{
			TryFinishMoveGroup();

			var moveGroup = new MoveGroup();
			command ??= new MoveCommand( destination );
			moveGroup.Initialize( this, command );

			MoveStack.Push( moveGroup );

			return moveGroup;
		}

		public MoveGroup PushMoveGroup( List<Vector3> destinations, IMoveCommand command = null )
		{
			if ( destinations.Count == 0 )
				return null;

			TryFinishMoveGroup();

			var moveGroup = new MoveGroup();
			command ??= new MoveCommand( destinations );
			moveGroup.Initialize( this, command );

			MoveStack.Push( moveGroup );

			return moveGroup;
		}

		public void ClearMoveStack()
		{
			foreach ( var group in MoveStack )
			{
				group.Remove( this );
			}

			MoveStack.Clear();
		}

		public bool CanOccupy( IOccupiableEntity occupiable )
		{
			var whitelist = occupiable.OccupiableItem.Occupiable.Whitelist;

			if ( whitelist.Count == 0 )
				return true;

			return whitelist.Contains( Item.UniqueId );
		}

		public bool SetOccupyTarget( IOccupiableEntity occupiable )
		{
			var modelEntity = (occupiable as ModelEntity);

			if ( modelEntity == null )
			{
				ClearTarget();
				return false;
			}

			ResetTarget();

			_target.Entity = modelEntity;
			_target.Radius = Pathfinder.NodeSize + Pathfinder.CollisionSize * 2;
			_target.Type = UnitTargetType.Occupy;

			OnTargetChanged();

			return true;
		}

		public bool SetDepositTarget( BuildingEntity building )
		{
			ResetTarget();

			_target.Entity = building;
			_target.Radius = Pathfinder.NodeSize + Pathfinder.CollisionSize * 2;
			_target.Type = UnitTargetType.Deposit;

			OnTargetChanged();

			return true;
		}

		public bool SetGatherTarget( ResourceEntity resource )
		{
			ResetTarget();

			_target.Entity = resource;
			_target.Radius = Pathfinder.NodeSize + Pathfinder.CollisionSize * 2;
			_target.Type = UnitTargetType.Gather;

			_gather.Type = resource.Resource;
			_gather.Entity = resource;
			_gather.Position = resource.Position;

			OnTargetChanged();

			return true;
		}

		public bool SetRepairTarget( BuildingEntity building )
		{
			ResetTarget();

			_target.Entity = building;
			_target.Radius = Pathfinder.NodeSize + Pathfinder.CollisionSize * 2;
			_target.Type = UnitTargetType.Repair;

			OnTargetChanged();

			return true;
		}

		public bool SetConstructTarget( BuildingEntity building )
		{
			ResetTarget();

			_target.Entity = building;
			_target.Radius = Pathfinder.NodeSize + Pathfinder.CollisionSize * 2;
			_target.Type = UnitTargetType.Construct;

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

			OnTargetChanged();
		}

		public float GetAttackRadius() => Item.AttackRadius;
		public float GetMinVerticalRange() => Item.MinVerticalRange;
		public float GetMaxVerticalRange() => Item.MaxVerticalRange;

		public float LookAtPosition( Vector3 position, float? interpolation = null, bool ignoreHeight = true )
		{
			var targetDirection = (position - Position);
			
			if ( ignoreHeight )
			{
				targetDirection = targetDirection.WithZ( 0f );
			}

			var targetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

			if ( interpolation.HasValue )
				Rotation = Rotation.Lerp( Rotation, targetRotation, interpolation.Value );
			else
				Rotation = targetRotation;

			return Rotation.Distance( targetRotation );
		}

		public float LookAtEntity( Entity target, float? interpolation = null, bool ignoreHeight = true )
		{
			return LookAtPosition( target.Position, interpolation );
		}

		public void OnVisibilityChanged( bool isVisible ) { }

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
			entity.EnableDrawOverWorld = true;

			Clothing.Add( entity );

			return entity;
		}

		public void OnMoveGroupDisposed( MoveGroup group )
		{
			if ( MoveStack.TryPeek( out var current ) && current == group )
			{
				_target.Position = null;
				_target.Follow = false;

				IsGathering = false;

				OnTargetChanged();

				MoveStack.Pop();

				MoveGroup next = null;

				while ( MoveStack.Count > 0 )
				{
					if ( MoveStack.TryPeek( out next ) )
					{
						if ( ! next.IsValid() )
						{
							MoveStack.Pop();
							continue;
						}
					}

					break;
				}

				if ( next != null )
				{
					next.Resume( this );
					return;
				}

				OnMoveStackEmpty();

				if ( MoveStack.Count == 0 )
					ClearTarget();
			}
		}

		public List<Vector3> GetDestinations( ModelEntity model )
		{
			return model.GetDestinations( Pathfinder );
		}

		public bool InVerticalRange( ISelectable other )
		{
			var entity = (other as Entity);
			if ( !entity.IsValid() ) return false;
			return InVerticalRange( entity );
		}

		public bool InVerticalRange( Entity other )
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

		[ClientRpc]
		protected void CreatePathParticles()
		{
			if ( _pathParticles != null )
			{
				_pathParticles.Destroy( true );
				_pathParticles = null;
			}

			if ( !Destination.IsNearZeroLength )
			{
				_pathParticles = Particles.Create( "particles/movement_path/movement_path.vpcf" );
				_pathParticles.SetEntity( 0, this );
				_pathParticles.SetPosition( 1, Destination.WithZ( Position.z ) );
				_pathParticles.SetPosition( 3, Player.TeamColor * 255f );
			}
		}

		protected void RemovePathParticles()
		{
			if ( _pathParticles != null )
			{
				_pathParticles.Destroy( true );
				_pathParticles = null;
			}
		}

		protected override void OnSelected()
		{
			base.OnSelected();

			CreatePathParticles();
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();

			RemovePathParticles();
		}

		protected override void OnQueueItemCompleted( QueueItem queueItem )
		{
			base.OnQueueItemCompleted( queueItem );

			if ( queueItem.Item is BaseUpgrade upgrade )
			{
				var changeWeaponTo = upgrade.ChangeWeaponTo;

				if ( !string.IsNullOrEmpty( changeWeaponTo ) )
				{
					ChangeWeapon( changeWeaponTo );
				}
			}
		}

		protected override void CreateAbilities()
		{
			base.CreateAbilities();

			if ( Item.CanDisband )
			{
				var disbandId = "ability_disband";
				var disband = RTS.Abilities.Create( disbandId );
				disband.Initialize( disbandId, this );
				Abilities[disbandId] = disband;
			}
		}

		protected override void OnItemNetworkIdChanged()
		{
			base.OnItemNetworkIdChanged();

			if ( Circle != null )
			{
				Circle.Size = GetDiameterXY( Item.CircleScale, true );
			}
		}

		protected override void OnPlayerAssigned( Player player )
		{
			if ( Item.UseRenderColor )
			{
				RenderColor = Player.TeamColor;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( IsClient )
			{
				Circle?.Delete();

				Fog.RemoveViewer( this );
				Fog.RemoveCullable( this );

				MiniMap.Instance.RemoveEntity( this );

				RemovePathParticles();

				return;
			}

			if ( Occupiable.IsValid() && Occupiable is IOccupiableEntity occupiable )
				occupiable.EvictUnit( this );

			if ( Player.IsValid() )
				Player.TakePopulation( Item.Population );

			_idleLoopSound.Stop();

			ClearMoveStack();
		}

		protected override void ServerTick()
		{
			base.ServerTick();

			Velocity = 0;

			var animator = Item.Animator;
			animator?.Reset();

			if ( !Occupiable.IsValid() )
			{
				if ( !IsUsingAbility() )
				{
					var isTargetInRange = IsTargetInRange();
					var isTargetValid = IsTargetValid();

					if ( _target.Type == UnitTargetType.Attack && isTargetValid )
					{
						ValidateAttackDistance();
					}

					if ( isTargetValid && isTargetInRange )
						TickInteractWithTarget();
					else
						TickMoveToTarget( isTargetValid, animator );

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
				if ( animator != null )
				{
					animator.Attacking = Weapon.LastAttack < 0.1f;
					animator.HoldType = Weapon.HoldType;
				}
			}

			// Let's see if our move group has finished now.
			TryFinishMoveGroup();

			// Network the current target type.
			TargetType = _target.Type;

			animator?.Apply( this );
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

			Scale = item.ModelScale;
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
					AddResistance( kv.Key, -kv.Value );
			}

			// Add the new base resistances.
			foreach ( var kv in item.Resistances )
				AddResistance( kv.Key, kv.Value );

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
				ChangeWeapon( item.Weapon );
			}

			Position = Position.WithZ( GetVerticalOffset() );

			base.OnItemChanged( item, oldItem );
		}

		public void ChangeWeapon( string name )
		{
			if ( Weapon.IsValid() )
			{
				Weapon.Delete();
				Weapon = null;
			}

			Weapon = Library.Create<Weapon>( name );
			Weapon.Attacker = this;
			Weapon.EnableDrawOverWorld = true;

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

		protected virtual void OnMoveStackEmpty()
		{
			if ( _target.Type == UnitTargetType.Gather
				|| _target.Type == UnitTargetType.Deposit )
			{
				FindTargetResource();
			}
		}

		protected virtual void CreateModifiers()
		{
			Modifiers = new UnitModifiers();
		}

		[Event.Frame]
		protected virtual void ClientFrame()
		{
			if ( Hud.Style.Opacity != RenderColor.a )
			{
				Hud.Style.Opacity = RenderColor.a;
				Hud.Style.Dirty();
			}

			Hud.SetActive( RenderColor.a > 0f );
		}

		protected override void ClientTick()
		{
			base.ClientTick();

			UpdatePathParticles();

			if ( Occupiable.IsValid() )
			{
				Circle.Alpha = 0f;
				RenderColor = RenderColor.WithAlpha( 0f );

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

			if ( IsLocalTeamGroup )
			{
				var isOnScreen = IsOnScreen();

				Circle.Alpha = isOnScreen ? 1f : 0f;
				RenderColor = RenderColor.WithAlpha( isOnScreen ? 1f : 0f );
				
				return;
			}

			RenderColor = RenderColor.WithAlpha( RenderColor.a.LerpTo( TargetAlpha, Time.Delta * 2f ) );

			for ( var i = 0; i < Children.Count; i++ )
			{
				if ( Children[i] is ModelEntity child )
				{
					child.RenderColor = child.RenderColor.WithAlpha( RenderColor.a );
				}
			}

			if ( Circle.IsValid() )
			{
				Circle.Alpha = RenderColor.a;
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

		protected virtual void UpdatePathParticles()
		{
			if ( _pathParticles == null ) return;

			if ( Destination.IsNearZeroLength )
			{
				RemovePathParticles();
				return;
			}

			_pathParticles.SetPosition( 1, Destination.WithZ( Position.z ) );
		}

		protected virtual void OnTargetChanged()
		{
			if ( Weapon.IsValid() )
			{
				Weapon.Target = _target.Entity;
				Weapon.Occupiable = Occupiable;
			}

			if ( IsSelected )
			{
				CreatePathParticles( To.Single( Player ) );
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

		private void TryFinishMoveGroup()
		{
			if ( MoveStack.TryPeek( out var group ) )
			{
				Destination = group.GetDestination();
				group.TryFinish( this );
			}
			else
			{
				Destination = Vector3.Zero;
			}
		}

		private void OnKillsChanged()
		{
			UpdateRank( Ranks.Find( Kills ) );
		}

		private void ResetTarget()
		{
			_target.Entity = null;
			_target.Position = null;
			_target.Follow = false;
			_target.Type = UnitTargetType.None;

			IsGathering = false;
		}

		private void FindTargetResource()
		{
			GatherCommand command;

			if ( MoveStack.TryPeek( out var group ) )
			{
				// Try to find a resource depo before we move on with our queue.
				if ( !FindResourceDepo() )
				{
					group.Resume( this );
				}

				return;
			}

			// If our last resource entity is valid just use that.
			if ( _gather.Entity.IsValid() )
			{
				command = new GatherCommand
				{
					Target = _gather.Entity
				};

				PushMoveGroup( GetDestinations( _gather.Entity ), command );

				return;
			}

			var entities = Physics.GetEntitiesInSphere( _gather.Position, 2000f );

			foreach ( var entity in entities )
			{
				if ( entity is ResourceEntity resource && resource.Resource == _gather.Type )
				{
					command = new GatherCommand
					{
						Target = resource
					};

					PushMoveGroup( GetDestinations( resource ), command );

					return;
				}
			}
		}

		private bool FindResourceDepo()
		{
			var buildings = Player.GetBuildings().Where( i => i.CanDepositResources );
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
			{
				var command = new DepositCommand
				{
					Target = closestDepo
				};

				PushMoveGroup( GetDestinations( closestDepo ), command );

				return true;
			}

			return false;
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
				if ( entity is ISelectable selectable && CanAttackTarget( selectable ) )
				{
					_targetBuffer.Add( selectable );
				}
			}

			_targetBuffer.OrderByDescending( s => s.GetAttackPriority() )
				.ThenBy( s => s.Position.Distance( searchPosition ) );

			if ( _targetBuffer.Count > 0 )
			{
				SetAttackTarget( _targetBuffer[0], false );
			}
		}

		private void TickFindTarget()
		{
			if ( IsMoveGroupValid() || !Weapon.IsValid() )
				return;

			if ( _target.Follow )
				return;

			if ( _nextFindTarget )
			{
				FindTargetEnemy();
				_nextFindTarget = 1f;
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
							TickConstruct( building );

						return;
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

			PushMoveGroup( position.Value );

			return false;
		}

		private void TickAbility()
		{
			var ability = UsingAbility;

			if ( ability == null || !ability.LookAtTarget )
				return;

			LookAtPosition( ability.TargetInfo.Origin, Time.Delta * Item.RotateToTargetSpeed );
		}

		private void UpdateFlockBuffer()
		{
			var bufferIndex = 0;
			var neighbours = Physics.GetEntitiesInSphere( Position, AgentRadius * 0.35f );

			foreach ( var neighbour in neighbours )
			{
				if ( neighbour is UnitEntity unit && ShouldOtherUnitFlock( unit ) )
				{
					_flockBuffer[bufferIndex] = unit;

					bufferIndex++;

					if ( bufferIndex >= _flockBuffer.Length )
						break;
				}
			}

			if ( bufferIndex < 8 )
			{
				Array.Clear( _flockBuffer, bufferIndex, _flockBuffer.Length - bufferIndex );
			}
		}

		private void UpdateFollowPosition( bool isTargetValid )
		{
			if ( !isTargetValid || !_target.Follow )
				return;

			_target.Position = _target.Entity.Position;
		}

		private void TickMoveToTarget( bool isTargetValid, UnitAnimator animator )
		{
			UpdateFollowPosition( isTargetValid );

			var nodeDirection = Vector3.Zero;
			var movementSpeed = GetSpeed();
			var direction = Vector3.Zero;

			if ( MoveStack.TryPeek( out var group ) )
			{
				var node = Pathfinder.CreateWorldPosition( Position );
				Pathfinder.DrawBox( node, Color.Green );

				if ( !IsAtDestination() )
				{
					if ( Item.UsePathfinder )
					{
						var offset = Pathfinder.CenterOffset.Normal;
						direction = group.GetDirection( Position );
						nodeDirection = (direction.Normal * offset).WithZ( 0f );
					}
					else
					{
						direction = (group.GetDestination() - Position).Normal;
						nodeDirection = direction.WithZ( 0f );
					}
				}
			}
			else if ( _target.Position.HasValue )
			{
				var straightDirection = (_target.Position.Value - Position).Normal.WithZ( 0f );

				if ( Pathfinder.IsAvailable( Position + (straightDirection * Pathfinder.NodeSize) ) )
					nodeDirection = straightDirection;
				else
					nodeDirection = Vector3.Zero;
			}

			if ( IsSlowTick() )
			{
				UpdateFlockBuffer();
			}

			var flocker = new Flocker();
			flocker.Setup( this, _flockBuffer, Position, movementSpeed );
			flocker.Flock( Position + direction * Math.Max( AgentRadius, Pathfinder.NodeSize ) );
			var steerDirection = flocker.Force.WithZ( 0f );

			if ( steerDirection.Length > 0 )
			{
				if ( !Item.UsePathfinder || Pathfinder.IsAvailable( Position + (steerDirection.Normal * Pathfinder.NodeSize) ) )
				{
					Velocity = steerDirection.ClampLength( movementSpeed );
				}
			}

			var acceleration = 4f;

			if ( Velocity.Length == 0 )
			{
				acceleration = 16f;
				Velocity = (nodeDirection * movementSpeed);
			}
			
			TargetVelocity = TargetVelocity.LerpTo( Velocity, Time.Delta * acceleration );
			Position += TargetVelocity * Time.Delta;

			var verticalOffset = GetVerticalOffset();
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
				if ( animator != null )
				{
					if ( movementSpeed >= 300f )
						animator.Speed = 1f;
					else
						animator.Speed = 0.5f;
				}

				Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( walkVelocity.Normal, Vector3.Up ), Time.Delta * 4f );
			}
		}

		private bool ShouldOtherUnitFlock( UnitEntity unit )
		{
			if ( unit.Velocity.Length > 0 )
				return true;

			if ( unit.TargetType == UnitTargetType.Gather )
				return false;
			else if ( unit.TargetType == UnitTargetType.Construct )
				return false;
			else if ( unit.TargetType == UnitTargetType.Repair )
				return false;

			return true;
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
			}
			else
			{
				building.UpdateRepair();
			}

			_nextRepairTime = 1f;
		}

		private void TickConstruct( BuildingEntity building )
		{
			if ( building.TouchingEntities.Count > 0 )
			{
				var blueprints = building.TouchingEntities
					.OfType<BuildingEntity>()
					.Where( v => v.IsBlueprint );

				foreach ( var blueprint in blueprints )
				{
					blueprint.CancelConstruction();
				}

				SpinSpeed = 0;

				return;
			}

			var itemNetworkId = building.ItemNetworkId;

			// Check if we can build this instantly.
			if ( !Player.InstantBuildCache.Contains( itemNetworkId ) )
			{
				if ( building.Item.BuildFirstInstantly )
				{
					Player.InstantBuildCache.Add( itemNetworkId );

					LookAtEntity( building );
					building.FinishConstruction();

					return;
				}
			}

			building.Health += (building.MaxHealth / building.Item.BuildTime * Time.Delta);
			building.Health = building.Health.Clamp( 0f, building.Item.MaxHealth );

			SpinSpeed = (building.MaxHealth / building.Health) * 200f;

			if ( building.Health == building.Item.MaxHealth )
			{
				LookAtEntity( building );
				building.FinishConstruction();
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

			_gather.LastGather = 0;
			IsGathering = true;

			TakeFrom( resource );

			if ( Carrying.TryGetValue( resource.Resource, out var carrying ) )
			{
				var maxCarry = resource.MaxCarry * Item.MaxCarryMultiplier;

				GatherProgress = (1f / maxCarry) * carrying;
				if ( carrying < maxCarry ) return;

				FindResourceDepo();
			}
		}
	}
}

