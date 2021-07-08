using Facepunch.RTS.Buildings;
using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Grid;
using Sandbox;
using Sandbox.UI;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer
	{
		[Net, Local] public RealTimeUntil NextGenerateResources { get; private set; }
		[Net] public List<UnitEntity> Occupants { get; private set; }
		[Net] public bool IsUnderConstruction { get; private set; }
		[Net] public float LineOfSight { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public uint LastQueueId { get; set; }
		public List<QueueItem> Queue { get; set; }
		public float NextFindTarget { get; private set; }
		public bool CanDepositResources => Item.CanDepositResources;
		public bool CanOccupyUnits => Occupants.Count < Item.MaxOccupants;
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
				unit.OnEnterBuilding( this );
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
				unit.OnLeaveBuilding( this );
				Occupants.Remove( unit );
			}
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

			SoundManager.Play( Player, "announcer.construction_complete" );
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

		public void PlaceNear( UnitEntity unit )
		{
			var bounds = GetDiameterXY( 0.75f );
			var pathfinder = unit.Pathfinder;
			var potentialNodes = new List<GridWorldPosition>();

			unit.Pathfinder.GetGridPositions( Position, bounds, potentialNodes );

			var freeLocations = potentialNodes
				.Where( v => pathfinder.IsAvailable( v ) )
				.ToList();

			if ( freeLocations.Count == 0 )
			{
				throw new Exception( "[BuildingEntity::PlaceNear] Unable to find a free location to spawn the unit!" );
			}

			var randomLocation = freeLocations[Rand.Int( freeLocations.Count - 1 )];
			var withHeightMap = pathfinder.GetPosition( randomLocation ) + new Vector3( 0f, 0f, pathfinder.GetHeight( randomLocation ) );

			// TODO: What we should do is have various attachments to buildings for spawn points that cannot be blocked.
			unit.Position = withHeightMap;
		}

		public void SpawnUnit( BaseUnit unit )
		{
			var entity = new UnitEntity();
			entity.Assign( Player, unit );

			if ( unit.UseRenderColor )
				entity.RenderColor = Player.TeamColor;

			PlaceNear( entity );
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
			}
			else
			{
				FogManager.RemoveViewer( this );
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
			FogManager.AddViewer( this );
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
				Item = ItemManager.Find<BaseItem>( itemId ),
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
