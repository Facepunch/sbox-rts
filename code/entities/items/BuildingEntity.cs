using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer, IOccupiableEntity
	{
		[Net, OnChangedCallback] public List<UnitEntity> Occupants { get; private set; }
		public bool CanOccupyUnits => Item.Occupiable.Enabled && Occupants.Count < Item.Occupiable.MaxOccupants;
		public IOccupiableItem OccupiableItem => Item;

		[Net, Local] public RealTimeUntil NextGenerateResources { get; private set; }
		[Net] public bool IsUnderConstruction { get; private set; }
		[Net] public float LineOfSightRadius { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public RealTimeUntil NextFindTarget { get; private set; }
		public bool CanDepositResources => Item.CanDepositResources;

		#region UI
		public EntityHudIconList OccupantsHud { get; private set; }
		public EntityHudBar GeneratorBar { get; private set; }
		public EntityHudBar HealthBar { get; private set; }
		#endregion

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );

			Occupants = new List<UnitEntity>();
		}

		public IList<UnitEntity> GetOccupantsList() => (Occupants as IList<UnitEntity>);

		public void UpdateConstruction()
		{
			Host.AssertServer();

			RenderAlpha = 0.25f + (0.75f / Item.MaxHealth) * Health;
			GlowColor = Color.Lerp( Color.Red, Color.Green, Health / Item.MaxHealth );
		}

		public float GetAttackRadius() => Item.AttackRadius;
		public float GetMaxVerticalRange() => Item.MaxVerticalRange;

		public bool IsTargetInRange()
		{
			if ( !Target.IsValid() ) return false;
			return (Target.IsValid() && Target.Position.Distance( Position ) < Item.AttackRadius);
		}

		public bool InVerticalRange( ISelectable other )
		{
			return (other.Position.z <= Item.MaxVerticalRange);
		}

		public bool OccupyUnit( UnitEntity unit )
		{
			Host.AssertServer();

			if ( CanOccupyUnits )
			{
				unit.OnOccupy( this );
				Occupants.Add( unit );
				OnOccupied( unit );
				return true;
			} 

			return false;
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

		public void EvictUnit( UnitEntity unit )
		{
			Host.AssertServer();

			if ( Occupants.Contains( unit ) )
			{
				unit.OnVacate( this );
				Occupants.Remove( unit );
				OnEvicted( unit );
			}
		}

		public void EvictAll()
		{
			for ( int i = 0; i < Occupants.Count; i++ )
			{
				var unit = Occupants[i];
				unit.OnVacate( this );
				OnEvicted( unit );
			}

			Occupants.Clear();
		}

		public void Attack( Entity target )
		{
			Target = target;
			OnTargetChanged();
		}

		public void ClearTarget()
		{
			Target = null;
			OnTargetChanged();
		}

		public void FinishConstruction()
		{
			Host.AssertServer();

			Player.AddDependency( Item );
			Player.MaxPopulation += Item.PopulationBoost;

			IsUnderConstruction = false;
			RenderAlpha = 1f;
			GlowActive = false;
			Health = Item.MaxHealth;

			AddAsFogViewer( To.Single( Player ) );

			Audio.Play( Player, "announcer.construction_complete" );

			Events.InvokeBuildingConstructed( Player, this );
		}

		public void StartConstruction()
		{
			Host.AssertServer();

			var radius = GetDiameterXY( 0.75f );

			foreach ( var pathfinder in PathManager.All )
			{
				pathfinder.UpdateCollisions( Position, radius );
			}

			IsUnderConstruction = true;
			RenderAlpha = 0.25f;
			GlowActive = true;
			GlowColor = Color.Red;
			Health = 1f;
		}

		public UnitEntity SpawnUnit( BaseUnit unit )
		{
			var entity = Items.Create( Player, unit );
			PlaceNear( entity );
			return entity;
		}

		public override int GetAttackPriority()
		{
			return Item.AttackPriority;
		}

		public override bool CanSelect()
		{
			return !IsUnderConstruction;
		}

		public override void OnKilled()
		{
			LifeState = LifeState.Dead;
			Delete();
		}

		public override void TakeDamage( DamageInfo info )
		{
			info = Resistances.Apply( info, Item.Resistances );

			DamageOccupants( info );

			base.TakeDamage( info );
		}

		public virtual Vector3? GetVacatePosition( UnitEntity unit )
		{
			return null;
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

		public virtual bool CanOccupantsAttack()
		{
			return true;
		}

		public override void UpdateHudComponents()
		{
			if ( Health <= MaxHealth * 0.9f || IsUnderConstruction )
			{
				HealthBar.SetProgress( Health / MaxHealth );
				HealthBar.SetActive( true );
			}
			else
			{
				HealthBar.SetActive( false );
			}

			if ( GeneratorBar != null )
			{
				var generator = Item.Generator;

				if ( !generator.PerOccupant || Occupants.Count > 0 )
				{
					var timeLeft = NextGenerateResources.Relative;
					GeneratorBar.SetProgress( 1f - (timeLeft / Item.Generator.Interval) );
					GeneratorBar.SetActive( true );
				}
				else
				{
					GeneratorBar.SetActive( false );
				}
			}

			OccupantsHud?.SetActive( Occupants.Count > 0 );

			base.UpdateHudComponents();
		}

		protected virtual void OnOccupied( UnitEntity unit )
		{
			
		}

		protected virtual void OnEvicted( UnitEntity unit )
		{

		}

		protected override void ServerTick()
		{
			base.ServerTick();

			TickGenerator();

			if ( Weapon.IsValid() && !IsUnderConstruction )
			{
				if ( Target.IsValid() )
				{
					if ( !IsTargetInRange() )
					{
						ClearTarget();
						return;
					}

					if ( Weapon.CanAttack() )
						Weapon.Attack();
				}

				if ( NextFindTarget )
				{
					FindTargetUnit();
				}
			}
		}

		protected virtual void TickGenerator()
		{
			var generator = Item.Generator;
			if ( generator == null ) return;

			if ( NextGenerateResources )
			{
				var multiplier = 1;

				if ( generator.PerOccupant )
					multiplier = Occupants.Count;

				if ( multiplier > 0 )
				{
					var resources = new Dictionary<ResourceType, int>();

					foreach ( var kv in generator.Resources )
						resources.Add( kv.Key, kv.Value * multiplier );

					ResourceHint.Send( Player, 2f, Position, resources, Color.Green );
					Player.GiveResources( resources );
				}

				NextGenerateResources = generator.Interval;
			}
		}

		protected virtual void OnOccupantsChanged()
		{
			if ( OccupantsHud == null ) return;

			OccupantsHud.DeleteChildren( true );

			foreach ( var occupant in Occupants )
			{
				var icon = OccupantsHud.AddChild<EntityHudIcon>();
				icon.Texture = occupant.Item.Icon;
			}
		}

		protected virtual void OnTargetChanged()
		{
			if ( Weapon.IsValid() )
			{
				Weapon.Target = Target;
			}
		}

		protected override void OnQueueItemCompleted( QueueItem queueItem )
		{
			if ( queueItem.Item is BaseUnit unit )
			{
				var entity = SpawnUnit( unit );
				Events.InvokeUnitTrained( Player, entity );
			}

			base.OnQueueItemCompleted( queueItem );
		}

		protected override void OnPlayerAssigned( Player player )
		{
			RenderColor = player.TeamColor;

			base.OnPlayerAssigned( player );
		}

		protected override void OnItemChanged( BaseBuilding item, BaseBuilding oldItem )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
			}

			if ( item.Generator != null )
				NextGenerateResources = item.Generator.Interval;
			else
				NextGenerateResources = 0;

			LineOfSightRadius = item.MinLineOfSight + CollisionBounds.Size.Length;
			LocalCenter = CollisionBounds.Center;
			MaxHealth = item.MaxHealth;
			Health = item.MaxHealth;

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

			base.OnItemChanged( item, oldItem );
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				var others = Player.GetBuildings( Item );

				if ( !others.Any() )
					Player.RemoveDependency( Item );

				if ( Player.IsValid() )
					Player.MaxPopulation -= Item.PopulationBoost;

				EvictAll();
			}
			else
			{
				var particles = Particles.Create( "particles/destruction_temp/destruction_temp.vpcf" );
				particles.SetPosition( 0, Position );
				particles.SetPosition( 1, new Vector3( GetDiameterXY( 1f, false ) * 0.5f, 0f, 0f ) );

				Fog.RemoveViewer( this );
			}

			base.OnDestroy();
		}

		private void FindTargetUnit()
		{
			var closestTarget = Physics.GetEntitiesInSphere( Position, Item.AttackRadius )
				.OfType<UnitEntity>()
				.Where( ( a ) => IsEnemy( a ) && InVerticalRange( a ) )
				.OrderByDescending( a => a.GetAttackPriority() )
				.ThenBy( a => a.Position.Distance( Position ) )
				.FirstOrDefault();

			if ( closestTarget.IsValid() )
			{
				Attack( closestTarget );
			}

			NextFindTarget = 0.5f;
		}

		[ClientRpc]
		private void AddAsFogViewer()
		{
			Fog.AddViewer( this );
		}

		protected override void AddHudComponents()
		{
			// We only want a generator bar is it's our building.
			if ( IsLocalPlayers && Item.Generator != null )
				GeneratorBar = Hud.AddChild<EntityHudBar>( "generator" );

			if ( IsLocalPlayers )
				OccupantsHud = Hud.AddChild<EntityHudIconList>();

			HealthBar = Hud.AddChild<EntityHudBar>( "health" );

			base.AddHudComponents();
		}
	}
}
