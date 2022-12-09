using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Sandbox;
using Sandbox.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer, IOccupiableEntity, IDamageable, IFogCullable, IMapIconEntity
	{
		[Net, Change] public IList<UnitEntity> Occupants { get; private set; }

		public IOccupiableItem OccupiableItem => Item;

		[Net, Local] public RealTimeUntil NextGenerateResources { get; private set; }
		[Net, Change( nameof( OnIsUnderConstructionChanged ) )] public bool IsUnderConstruction { get; set; }
		public HashSet<Entity> TouchingEntities { get; private set; }
		[Net, Change] public Vector3 RallyPosition { get; set; }
		public IMoveCommand RallyCommand { get; set; }
		[Net] public float LineOfSightRadius { get; private set; }
		[Net] public bool IsBlueprint { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public RealTimeUntil NextFindTarget { get; private set; }
		public bool CanSetRallyPoint { get; private set; }
		public float TargetAlpha { get; private set; }
		public bool HasBeenSeen { get; set; }
		public bool IsVisible { get; set; }
		public Color IconColor => IsBlueprint ? Player.TeamColor.WithAlpha( 0.4f ) : Player.TeamColor;

		public bool CanDepositResources
		{
			get
			{
				if ( IsUnderConstruction ) return false;
				return Item.CanDepositResources;
			}
		}

		public bool CanOccupyUnits
		{
			get
			{
				if ( IsUnderConstruction ) return false;
				return Item.Occupiable.Enabled && Occupants.Count < Item.Occupiable.MaxOccupants;
			}
		}

		private RealTimeUntil NextConstructionSound { get; set; }
		private Sound ConstructionSound { get; set; }
		private Sound? GeneratorSound { get; set; }

		#region UI
		public EntityHudIconList OccupantsHud { get; private set; }
		public EntityHudBar GeneratorBar { get; private set; }
		public EntityHudBar HealthBar { get; private set; }
		#endregion

		private HistoryBuilding HistoryBuilding;
		private Particles RallyMarker;

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable", "blueprint" );

			TouchingEntities = new();
			Occupants = new List<UnitEntity>();

			if ( IsServer )
			{
				EnableSolidCollisions = false;
				IsUnderConstruction = true;
				IsBlueprint = true;
				EnableTouch = true;
				Health = 1f;
			}
		}

		public IList<UnitEntity> GetOccupantsList() => (Occupants as IList<UnitEntity>);

		public void OnVisibilityChanged( bool isVisible )
		{
			if ( IsLocalTeamGroup )
			{
				TargetAlpha = 1f;
				return;
			}

			if ( IsBlueprint )
			{
				TargetAlpha = 0f;
				return;
			}

			if ( HasBeenSeen && !isVisible )
			{
				if ( !HistoryBuilding.IsValid() )
				{
					HistoryBuilding = new HistoryBuilding();
					HistoryBuilding.Copy( this );
				}

				EnableDrawing = true;
				TargetAlpha = 1f;
			}
			else
			{
				if ( isVisible && HistoryBuilding.IsValid() )
				{
					HistoryBuilding.Delete();
					HistoryBuilding = null;
				}
				else
				{
					TargetAlpha = isVisible ? 1f : 0f;
				}
			}
		}

		public int GetActiveConstructorCount()
		{
			return FindInSphere( Position, GetDiameterXY( 2f ) )
				.OfType<UnitEntity>()
				.Where( u => u.TargetType == UnitTargetType.Construct && u.TargetEntity == this )
				.Count();
		}

		public void MakeVisible( bool isVisible ) { }

		public void CancelConstruction()
		{
			ResourceHint.Send( Player, 2f, Position, Item.Costs, Color.Green );
			Player.GiveResources( Item );
			Delete();
		}

		public void UpdateConstruction()
		{
			Host.AssertServer();

			if ( IsBlueprint )
			{
				UpgradeFromBlueprint();
			}

			if ( Item.ConstructionSounds.Length > 0 && NextConstructionSound )
			{
				var sound = Rand.FromArray( Item.ConstructionSounds );

				ConstructionSound.Stop();
				ConstructionSound = PlaySound( sound );

				NextConstructionSound = 3f;
			}
		}

		public void UpdateRepair()
		{
			Host.AssertServer();

			if ( Item.ConstructionSounds.Length > 0 && NextConstructionSound )
			{
				var sound = Rand.FromArray( Item.ConstructionSounds );

				ConstructionSound.Stop();
				ConstructionSound = PlaySound( sound );

				NextConstructionSound = 3f;
			}
		}

		public float GetAttackRadius() => Item.AttackRadius;
		public float GetMinVerticalRange() => Item.MinVerticalRange;
		public float GetMaxVerticalRange() => Item.MaxVerticalRange;

		public bool IsTargetInRange()
		{
			if ( !Target.IsValid() ) return false;
			return (Target.IsValid() && Target.Position.Distance( Position ) < Item.AttackRadius);
		}

		public void SetRallyCommand( IMoveCommand command, Vector3 position )
		{
			RallyCommand = command;
			RallyPosition = position;
		}

		public bool IsDamaged()
		{
			return Health < MaxHealth;
		}

		public bool InVerticalRange( Entity other )
		{
			var selfPosition = Position;
			var minVerticalRange = Item.MinVerticalRange;
			var maxVerticalRange = Item.MaxVerticalRange;
			var distance = Math.Abs( selfPosition.z - other.Position.z );
			return (distance >= minVerticalRange && distance <= maxVerticalRange);
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

		public void Kill()
		{
			var particles = Particles.Create( "particles/destruction_temp/destruction_temp.vpcf" );
			particles.SetPosition( 0, Position );
			particles.SetPosition( 1, new Vector3( GetDiameterXY( 1f, false ) * 0.5f, 0f, 0f ) );

			Item.PlayDestroySound( this );
			LifeState = LifeState.Dead;
			Delete();
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

			if ( IsBlueprint )
			{
				UpgradeFromBlueprint();
			}

			AddDependencies( Item );

			Player.MaxPopulation += Item.PopulationBoost;

			IsUnderConstruction = false;
			Health = Item.MaxHealth;

			AddAsFogViewer( To.Everyone );

			Audio.Play( Player, "announcer.construction_complete" );

			Item.PlayBuiltSound( this );

			Events.InvokeBuildingConstructed( Player, this );
		}

		public void FinishRepair()
		{
			Host.AssertServer();

			Item.PlayBuiltSound( this );
		}

		public void UpdateCollisions()
		{
			var radius = GetDiameterXY( 0.75f );

			foreach ( var pathfinder in PathManager.All )
			{
				pathfinder.UpdateCollisions( Position, radius );
			}
		}

		public UnitEntity SpawnUnit( BaseUnit unit )
		{
			var entity = Items.Create( Player, unit );
			PlaceNear( entity );
			return entity;
		}

		public override void ShowTooltip()
		{
			if ( IsLocalPlayers && CanOccupyUnits )
			{
				var selected = Player.GetSelected<UnitEntity>();
				var canAnyEnter = false;

				for ( int i = 0; i < selected.Count; i++ )
				{
					var unit = selected[i];

					if ( unit.CanOccupy( this ) )
					{
						canAnyEnter = true;
						break;
					}
				}

				if ( canAnyEnter )
				{
					var tooltip = GenericTooltip.Instance;

					tooltip.Update( Item.Name, "+iv_sprint++iv_attack2 to occupy this building.", "occupiable" );
					tooltip.Hover( this );
					tooltip.Show( 0.5f );
				}
			}

			base.ShowTooltip();
		}

		public override void ClientSpawn()
		{
			if ( IsUnderConstruction )
			{
				var glow = Components.GetOrCreate<Glow>();
				glow.Enabled = true;
				glow.Color = Color.Red;
			}

			if ( !IsLocalTeamGroup )
			{
				Fog.AddCullable( this );
				TargetAlpha = 0f;
			}
			else
			{
				TargetAlpha = 1f;
			}

			RenderColor = RenderColor.WithAlpha( 0f );

			var icon = MiniMap.Instance.AddEntity( this, "building" );
			icon.SetSize( CollisionBounds * 0.9f );

			base.ClientSpawn();
		}

		public override bool CanBeAttacked()
		{
			return !IsBlueprint;
		}

		public override int GetAttackPriority()
		{
			return Item.AttackPriority;
		}

		public override void CancelAction()
		{
			if ( IsUnderConstruction )
			{
				Audio.Play( Player, "rts.beepvibrato" );
				CancelConstruction();
			}
		}

		public override void StartTouch( Entity other )
		{
			if ( !other.IsWorld )
				TouchingEntities.Add( other );

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
			if ( !other.IsWorld )
				TouchingEntities.Remove( other );

			base.EndTouch( other );
		}

		public override bool CanSelect()
		{
			return !IsUnderConstruction;
		}

		public override void OnKilled()
		{
			Kill();
		}

		public override void TakeDamage( DamageInfo info )
		{
			info = Resistances.Apply( info, Item.Resistances );

			DamageOccupants( info );

			base.TakeDamage( info );
		}

		public virtual bool ShouldShowOnMap()
		{
			return IsLocalTeamGroup || IsVisible;
		}

		public virtual void DoImpactEffects( Vector3 position, Vector3 normal )
		{

		}

		public virtual void CreateDamageDecals( Vector3 position )
		{

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

		protected virtual void OnIsUnderConstructionChanged( bool newValue )
		{
			if ( IsLocalPlayers )
			{
				var glow = Components.GetOrCreate<Glow>();
				glow.Enabled = newValue;
			}
		}

		protected virtual void OnOccupied( UnitEntity unit )
		{
			UpdateLineOfSight();
		}

		protected virtual void OnEvicted( UnitEntity unit )
		{
			UpdateLineOfSight();
		}

		[Event.Client.Frame]
		protected virtual void ClientFrame()
		{
			if ( IsLocalTeamGroup ) return;

			if ( Hud.Style.Opacity != RenderColor.a )
			{
				Hud.Style.Opacity = RenderColor.a;
				Hud.Style.Dirty();
			}

			Hud.SetActive( EnableDrawing && RenderColor.a > 0f );
		}

		protected override void ClientTick()
		{
			base.ClientTick();

			var targetAlpha = TargetAlpha;

			if ( IsUnderConstruction )
			{
				if ( IsLocalTeamGroup || !IsBlueprint )
					targetAlpha = MathF.Min( 0.5f + (0.5f / Item.MaxHealth) * Health, targetAlpha );
				else
					targetAlpha = 0f;

				var glow = Components.GetOrCreate<Glow>();
				glow.Color = Color.Lerp( Color.Red, Color.Green, Health / Item.MaxHealth );
			}

			if ( IsLocalTeamGroup )
			{
				RenderColor = RenderColor.WithAlpha( targetAlpha );
				return;
			}

			RenderColor = RenderColor.WithAlpha( RenderColor.a.LerpTo( targetAlpha, Time.Delta * 2f ) );
		}

		protected override void CreateAbilities()
		{
			base.CreateAbilities();

			if ( Item.CanDemolish )
			{
				var demolishId = "ability_demolish";
				var demolish = RTS.Abilities.Create( demolishId );
				demolish.Initialize( demolishId, this );
				Abilities[demolishId] = demolish;
			}
		}

		protected override void ServerTick()
		{
			base.ServerTick();

			if ( IsUnderConstruction )
				return;

			if ( !IsSlowTick() )
				return;

			TickGenerator();

			if ( Weapon.IsValid() )
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

				FindTargetUnit();
			}
		}

		protected virtual void TickGenerator()
		{
			var generator = Item.Generator;
			if ( generator == null ) return;

			var multiplier = 1;

			if ( generator.PerOccupant )
				multiplier = Occupants.Count;

			if ( multiplier == 0 ) return;

			if ( !GeneratorSound.HasValue && !string.IsNullOrEmpty( generator.LoopSound ) )
			{
				GeneratorSound = PlaySound( generator.LoopSound );
			}

			if ( NextGenerateResources )
			{
				var resources = new Dictionary<ResourceType, int>();

				foreach ( var kv in generator.Resources )
					resources.Add( kv.Key, kv.Value * multiplier );

				ResourceHint.Send( Player, 2f, Position, resources, Color.Green );
				Player.GiveResources( resources );

				NextGenerateResources = generator.Interval;

				if ( !string.IsNullOrEmpty( generator.FinishSound ) )
				{
					PlaySound( generator.FinishSound );
				}
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

		protected virtual void UpdateLineOfSight()
		{
			LineOfSightRadius = Item.MinLineOfSight + CollisionBounds.Size.Length;

			if ( Item.Occupiable.Enabled && Occupants.Count > 0 )
			{
				var lineOfSightPerOccupant = ((Item.Occupiable.MaxLineOfSightAdd - Item.Occupiable.MinLineOfSightAdd) / Item.Occupiable.MaxOccupants) * Occupants.Count;
				var addedLineOfSight = Item.Occupiable.MinLineOfSightAdd + lineOfSightPerOccupant;
				LineOfSightRadius += addedLineOfSight;
			}
		}

		protected override void OnQueueItemCompleted( QueueItem queueItem )
		{
			if ( queueItem.Item is BaseUnit unit )
			{
				var entity = SpawnUnit( unit );

				if ( RallyCommand != null )
				{
					entity.ClearMoveStack();
					entity.PushMoveGroup( RallyCommand );
				}

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
			if ( oldItem != null && !IsUnderConstruction )
			{
				RemoveDependencies( oldItem );
				AddDependencies( item );
			}

			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
			}

			if ( GeneratorSound.HasValue )
			{
				GeneratorSound.Value.Stop();
				GeneratorSound = null;
			}

			if ( item.Generator != null )
				NextGenerateResources = item.Generator.Interval;
			else
				NextGenerateResources = 0;

			UpdateLineOfSight();

			LocalCenter = CollisionBounds.Center;
			MaxHealth = item.MaxHealth;

			var canTrainUnits = false;

			foreach ( var q in item.Queueables )
			{
				var queueable = Items.Find<BaseUnit>( q );

				if ( queueable != null )
				{
					canTrainUnits = true;
					break;
				}
			}

			CanSetRallyPoint = (item.CanSetRallyPoint && canTrainUnits);

			if ( Weapon.IsValid() ) Weapon.Delete();

			if ( !string.IsNullOrEmpty( item.Weapon ) )
			{
				Weapon = TypeLibrary.Create<Weapon>( item.Weapon );
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

		protected override void AddHudComponents()
		{
			// We only want a generator bar is it's our building.
			if ( IsLocalPlayers && Item.Generator != null )
				GeneratorBar = Hud.AddChild<EntityHudBar>( "generator" );

			OccupantsHud = Hud.AddChild<EntityHudIconList>();
			HealthBar = Hud.AddChild<EntityHudBar>( "health" );

			base.AddHudComponents();
		}

		protected override void OnSelected()
		{
			base.OnSelected();

			ShowRallyMarker();
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();

			HideRallyMarker();
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				RemoveDependencies( Item );

				if ( Player.IsValid() )
				{
					if ( !IsUnderConstruction )
						Player.MaxPopulation -= Item.PopulationBoost;
				}

				if ( GeneratorSound.HasValue )
				{
					GeneratorSound.Value.Stop();
					GeneratorSound = null;
				}

				UpdateCollisions();
				EvictAll();
			}
			else
			{
				MiniMap.Instance.RemoveEntity( this );

				Fog.RemoveCullable( this );
				Fog.RemoveViewer( this );
			}

			base.OnDestroy();
		}

		private void UpgradeFromBlueprint()
		{
			EnableSolidCollisions = true;
			Tags.Remove( "blueprint" );
			UpdateCollisions();
			IsBlueprint = false;
		}

		private void AddDependencies( BaseBuilding item )
		{
			Player.AddDependency( item );

			var proxyList = item.ActsAsProxyFor;

			for ( var i = 0; i < proxyList.Length; i++ )
			{
				var proxyItem = Items.Find<BaseBuilding>( proxyList[i] );
				if ( proxyItem == null || Player.HasDependency( proxyItem ) ) continue;

				AddDependencies( proxyItem );
			}
		}

		private void RemoveDependencies( BaseBuilding item )
		{
			var others = Player.GetBuildingsProxiesIncluded( item );

			if ( others.Count() <= 1 )
				Player.RemoveDependency( item );

			var proxyList = item.ActsAsProxyFor;

			for ( var i = 0; i < proxyList.Length; i++ )
			{
				var proxyItem = Items.Find<BaseBuilding>( proxyList[i] );
				if ( proxyItem == null || !Player.HasDependency( proxyItem ) ) continue;

				RemoveDependencies( proxyItem );
			}
		}

		private void FindTargetUnit()
		{
			if ( !NextFindTarget ) return;

			var closestTarget = FindInSphere( Position, Item.AttackRadius )
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

		private void OnRallyPositionChanged()
		{
			if ( IsSelected && IsLocalPlayers )
			{
				ShowRallyMarker();
			}
		}

		private void HideRallyMarker()
		{
			RallyMarker?.Destroy();
			RallyMarker = null;
		}

		private void ShowRallyMarker()
		{
			if ( RallyPosition.IsNearZeroLength )
				return;

			HideRallyMarker();

			RallyMarker = Particles.Create( "particles/flag_marker/flag_marker.vpcf" );
			RallyMarker.SetPosition( 0, RallyPosition );
			RallyMarker.SetPosition( 1, Player.TeamColor * 255f );
		}

		[ClientRpc]
		private void AddAsFogViewer()
		{
			if ( IsLocalTeamGroup )
			{
				Fog.AddViewer( this );
			}
		}
	}
}
