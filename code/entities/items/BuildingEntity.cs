using Facepunch.RTS.Buildings;
using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer, IOccupiableEntity
	{
		[Net] public List<UnitEntity> Occupants { get; private set; }
		public bool CanOccupyUnits => Occupants.Count < Item.MaxOccupants;
		public IOccupiableItem OccupiableItem => Item;

		[Net, Local] public RealTimeUntil NextGenerateResources { get; private set; }
		[Net] public bool IsUnderConstruction { get; private set; }
		[Net] public float LineOfSight { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public uint LastQueueId { get; set; }
		public List<QueueItem> Queue { get; set; }
		public float NextFindTarget { get; private set; }
		public bool CanDepositResources => Item.CanDepositResources;
		public Placeholder Placeholder { get; private set; }

		#region UI
		public EntityHudBar GeneratorBar { get; private set; }
		public EntityHudBar HealthBar { get; private set; }
		#endregion

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );

			Occupants = new List<UnitEntity>();
			Queue = new List<QueueItem>();
		}

		public IList<UnitEntity> GetOccupantsList() => (Occupants as IList<UnitEntity>);

		public void UpdateConstruction()
		{
			Host.AssertServer();

			RenderAlpha = 0.25f + (0.75f / Item.MaxHealth) * Health;
			GlowColor = Color.Lerp( Color.Red, Color.Green, Health / Item.MaxHealth );
		}

		public bool IsTargetInRange()
		{
			if ( !Target.IsValid() ) return false;

			return (Target.IsValid() && Target.Position.Distance( Position ) < Item.AttackRange);
		}

		public bool OccupyUnit( UnitEntity unit )
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

		public void QueueItem( BaseItem item )
		{
			Host.AssertServer();

			LastQueueId++;

			var queueItem = new QueueItem
			{
				Item = item,
				Id = LastQueueId
			};

			Queue.Add( queueItem );

			AddToQueue( To.Single( Player ), LastQueueId, item.NetworkId );

			if ( Queue.Count == 1 )
			{
				queueItem.Start();
				StartQueueItem( To.Single( Player ), LastQueueId, queueItem.FinishTime );
			}
		}

		public void SpawnUnit( BaseUnit unit )
		{
			var entity = new UnitEntity();
			entity.Assign( Player, unit );

			if ( unit.UseRenderColor )
				entity.RenderColor = Player.TeamColor;

			PlaceNear( entity );
		}

		public Vector3? GetVacatePosition( UnitEntity unit )
		{
			return null;
		}

		public BaseItem UnqueueItem( uint queueId )
		{
			Host.AssertServer();

			BaseItem removedItem = default;

			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					removedItem = Queue[i].Item;
					Queue.RemoveAt( i );
					break;
				}
			}

			RemoveFromQueue( To.Single( Player ), queueId );

			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime == 0f )
				{
					firstItem.Start();
					StartQueueItem( To.Single( Player ), firstItem.Id, firstItem.FinishTime );
				}
			}

			return removedItem;
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

		public virtual void DamageOccupants( DamageInfo info )
		{
			var scale = Item.OccupantDamageScale;
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

		protected override void ServerTick()
		{
			base.ServerTick();

			TickGenerator();

			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime > 0f && RTS.Game.ServerTime >= firstItem.FinishTime )
				{
					OnQueueItemCompleted( firstItem );
					UnqueueItem( firstItem.Id );
					firstItem.Item.OnCreated( Player );
				}
			}

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

				if ( Time.Now >= NextFindTarget )
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

		protected virtual void OnTargetChanged()
		{
			if ( Weapon.IsValid() )
				Weapon.Target = Target;
		}

		protected virtual void OnQueueItemCompleted( QueueItem queueItem )
		{
			if ( queueItem.Item is BaseTech tech )
			{
				Player.AddDependency( tech );
			}
			else if ( queueItem.Item is BaseUnit unit )
			{
				SpawnUnit( unit );
			}
		}

		protected override void OnPlayerAssigned( Player player )
		{
			RenderColor = player.TeamColor;

			base.OnPlayerAssigned( player );
		}

		protected override void OnItemChanged( BaseBuilding item )
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

			LineOfSight = item.MinLineOfSight + CollisionBounds.Size.Length;
			MaxHealth = item.MaxHealth;
			Health = item.MaxHealth;

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

			base.OnItemChanged( item );
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				var others = Player.GetBuildings( Item );

				if ( others.Count == 0 )
					Player.RemoveDependency( Item );

				if ( Player.IsValid() )
					Player.MaxPopulation -= Item.PopulationBoost;

				EvictAll();
			}
			else
			{
				Fog.RemoveViewer( this );
			}

			base.OnDestroy();
		}

		private void FindTargetUnit()
		{
			var closestTarget = Physics.GetEntitiesInSphere( Position, Item.AttackRange )
				.OfType<UnitEntity>()
				.Where( ( a ) => IsEnemy( a ) )
				.OrderBy( ( a ) => a.Position.Distance( Position ) )
				.FirstOrDefault();

			if ( closestTarget.IsValid() )
			{
				Attack( closestTarget );
			}

			NextFindTarget = Time.Now + 0.5f;
		}

		[ClientRpc]
		private void StartQueueItem( uint queueId, float finishTime )
		{
			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue[i].FinishTime = finishTime;
					return;
				}
			}
		}

		[ClientRpc]
		private void AddAsFogViewer()
		{
			Fog.AddViewer( this );
		}

		[ClientRpc]
		private void RemoveFromQueue( uint queueId )
		{
			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue.RemoveAt( i );
					return;
				}
			}
		}

		[ClientRpc]
		private void AddToQueue( uint queueId, uint itemId )
		{
			var queueItem = new QueueItem
			{
				Item = Items.Find<BaseItem>( itemId ),
				Id = queueId
			};

			Queue.Add( queueItem );
		}

		protected override void AddHudComponents()
		{
			// We only want a generator bar is it's our building.
			if ( IsLocalPlayers && Item.Generator != null )
			{
				GeneratorBar = UI.AddChild<EntityHudBar>( "generator" );
			}

			HealthBar = UI.AddChild<EntityHudBar>( "health" );

			base.AddHudComponents();
		}

		protected override void UpdateHudComponents()
		{
			if ( Health <= MaxHealth * 0.9f || IsUnderConstruction )
			{
				HealthBar.Foreground.Style.Width = Length.Fraction( Health / MaxHealth );
				HealthBar.Foreground.Style.Dirty();
				HealthBar.SetClass( "hidden", false );
			}
			else
			{
				HealthBar.SetClass( "hidden", true );
			}

			if ( GeneratorBar != null )
			{
				var generator = Item.Generator;

				if ( !generator.PerOccupant || Occupants.Count > 0 )
				{
					var timeLeft = NextGenerateResources.Relative;
					GeneratorBar.Foreground.Style.Width = Length.Fraction( 1f - (timeLeft / Item.Generator.Interval) );
					GeneratorBar.Foreground.Style.Dirty();
					GeneratorBar.SetClass( "hidden", false );
				}
				else
				{
					GeneratorBar.SetClass( "hidden", true );
				}
			}

			base.UpdateHudComponents();
		}
	}
}
