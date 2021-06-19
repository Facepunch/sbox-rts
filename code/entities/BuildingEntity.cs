using RTS.Buildings;
using RTS.Tech;
using RTS.Units;
using Sandbox;
using Sandbox.UI;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>, IFogViewer
	{
		[Net] public bool IsUnderConstruction { get; private set; }
		[Net] public float LineOfSight { get; private set; }
		[Net] public Weapon Weapon { get; private set; }
		[Net] public Entity Target { get; private set; }
		public uint LastQueueId { get; set; }
		public List<QueueItem> Queue { get; set; }
		public bool CanDepositResources => Item.CanDepositResources;

		#region UI
		public EntityHudBar HealthBar { get; private set; }
		#endregion

		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );
			Queue = new();
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

		public void Attack( Entity target )
		{
			Weapon.Target = target;
			Target = target;
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
		}

		public void StartConstruction()
		{
			Host.AssertServer();

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

			var buildingMaxSize = CollisionBounds.Size.Length;
			var availablePoint = NavMesh.GetPointWithinRadius( Position, buildingMaxSize * 0.5f, buildingMaxSize );

			// TODO: What we should do is have various attachments to buildings for spawn points that cannot be blocked.
			if ( availablePoint.HasValue )
			{
				entity.Position = availablePoint.Value;
			}
			else
			{
				Log.Error( "Unable to find a location for the unit " + unit.Name + " to spawn!" );
			}
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

		[Event.Tick.Server]
		public virtual void ServerTick()
		{
			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime > 0f && Game.Instance.ServerTime >= firstItem.FinishTime )
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
						Weapon.Target = null;
						Target = null;
						return;
					}

					if ( Weapon.CanAttack() )
						Weapon.Attack();
				}
				else
				{
					FindTargetUnit();
				}
			}
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
					Weapon.SetParent( this, true );
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
				FogManager.Instance.RemoveViewer( this );
			}

			base.OnDestroy();
		}

		private void FindTargetUnit()
		{
			var entities = Physics.GetEntitiesInSphere( Position, Item.AttackRange );

			foreach ( var entity in entities )
			{
				if ( entity is UnitEntity unit )// && IsEnemy( unit ) )
				{
					Attack( unit );
					return;
				}
			}
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
			FogManager.Instance.AddViewer( this );
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
				Item = ItemManager.Instance.Find<BaseItem>( itemId ),
				Id = queueId
			};

			Queue.Add( queueItem );
		}

		protected override void AddHudComponents()
		{
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

			base.UpdateHudComponents();
		}
	}
}
