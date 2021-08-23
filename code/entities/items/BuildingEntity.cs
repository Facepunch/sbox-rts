using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer, IOccupiableEntity, IDamageable, IFogCullable
	{
		[Net, OnChangedCallback] public List<UnitEntity> Occupants { get; private set; }

		public IOccupiableItem OccupiableItem => Item;

		[Net, Local] public RealTimeUntil NextGenerateResources { get; private set; }
		[Net, OnChangedCallback] public bool IsUnderConstruction { get; private set; }
		public HashSet<Entity> TouchingEntities { get; private set; }
		[Net] public float LineOfSightRadius { get; private set; }
		[Net] public bool IsBlueprint { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public RealTimeUntil NextFindTarget { get; private set; }
		public float TargetAlpha { get; private set; }
		public bool HasBeenSeen { get; set; }

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

		private HistoryBuilding _historyBuilding;

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );

			TouchingEntities = new();
			Occupants = new List<UnitEntity>();

			if ( IsServer )
			{
				EnableSolidCollisions = false;
				IsUnderConstruction = true;
				IsBlueprint = true;
				EnableTouch = true;
				GlowColor = Color.Red;
				Health = 1f;
			}
		}

		public IList<UnitEntity> GetOccupantsList() => (Occupants as IList<UnitEntity>);

		public void MakeVisible( bool isVisible, bool wasVisible )
		{
			if ( HasBeenSeen && !isVisible && !wasVisible )
			{
				if ( !_historyBuilding.IsValid() )
				{
					_historyBuilding = new HistoryBuilding();
					_historyBuilding.Copy( this );
				}

				EnableDrawing = true;
				TargetAlpha = 1f;
			}
			else
			{
				if ( isVisible && _historyBuilding.IsValid() )
				{
					_historyBuilding.Delete();
					_historyBuilding = null;
				} else {
					TargetAlpha = isVisible ? 1f : 0f;
				}
			}
		}

		public void CancelConstruction()
		{
			ResourceHint.Send( Player, 2f, Position, Item.Costs, Color.Green );
			Player.GiveResources( Item );
			Delete();
		}

		public void UpdateConstruction()
		{
			Host.AssertServer();

			GlowColor = Color.Lerp( Color.Red, Color.Green, Health / Item.MaxHealth );

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

		public bool IsDamaged()
		{
			return Health < MaxHealth;
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

		public void Kill()
		{
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
			IsBlueprint = false;
			Health = Item.MaxHealth;

			AddAsFogViewer( To.Single( Player ) );

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

		public override void ClientSpawn()
		{
			RenderAlpha = 0f;

			Fog.AddCullable( this );

			base.ClientSpawn();
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
			TouchingEntities.Add( other );

			base.StartTouch( other );
		}

		public override void EndTouch( Entity other )
		{
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

		protected virtual void OnOccupied( UnitEntity unit )
		{
			
		}

		protected virtual void OnEvicted( UnitEntity unit )
		{

		}

		protected override void ClientTick()
		{
			base.ClientTick();

			var targetAlpha = TargetAlpha;

			if ( IsUnderConstruction && ( IsLocalPlayers || !IsBlueprint ))
			{
				targetAlpha = MathF.Min( 0.25f + (0.75f / Item.MaxHealth) * Health, targetAlpha );
			}

			if ( IsLocalPlayers )
			{
				RenderAlpha = targetAlpha;
				return;
			}

			RenderAlpha = RenderAlpha.LerpTo( targetAlpha, Time.Delta * 2f );

			if ( Hud.Style.Opacity != RenderAlpha )
			{
				Hud.Style.Opacity = RenderAlpha;
				Hud.Style.Dirty();
			}

			Hud.SetActive( EnableDrawing && RenderAlpha > 0f );
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

			if ( IsUnderConstruction ) return;

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
			if ( oldItem != null && !IsUnderConstruction )
			{
				RemoveDependencies( oldItem );
				AddDependencies( item );
			}

			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
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

			LineOfSightRadius = item.MinLineOfSight + CollisionBounds.Size.Length;
			LocalCenter = CollisionBounds.Center;
			MaxHealth = item.MaxHealth;

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

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				RemoveDependencies( Item );

				if ( Player.IsValid() )
					Player.MaxPopulation -= Item.PopulationBoost;

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
				var particles = Particles.Create( "particles/destruction_temp/destruction_temp.vpcf" );
				particles.SetPosition( 0, Position );
				particles.SetPosition( 1, new Vector3( GetDiameterXY( 1f, false ) * 0.5f, 0f, 0f ) );

				Fog.RemoveCullable( this );
				Fog.RemoveViewer( this );
			}

			base.OnDestroy();
		}

		private void UpgradeFromBlueprint()
		{
			EnableSolidCollisions = true;
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
				if ( proxyItem == null ) continue;

				Player.AddDependency( proxyItem );
			}
		}

		private void RemoveDependencies( BaseBuilding item )
		{
			var others = Player.GetBuildings( item );

			if ( others.Count() == 1 )
				Player.RemoveDependency( item );

			var proxyList = item.ActsAsProxyFor;

			for ( var i = 0; i < proxyList.Length; i++ )
			{
				var proxyItem = Items.Find<BaseBuilding>( proxyList[i] );
				if ( proxyItem == null ) continue;

				others = Player.GetBuildings( proxyItem );

				if ( others.Count() == 1 )
					Player.RemoveDependency( proxyItem );
			}
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

		private void OnIsUnderConstructionChanged()
		{
			if ( IsLocalPlayers )
			{
				GlowActive = IsUnderConstruction;
			}
		}
	}
}
