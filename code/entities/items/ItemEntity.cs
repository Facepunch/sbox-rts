using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.Extensions;
using Gamelib.FlowFields.Grid;
using Facepunch.RTS;
using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Facepunch.RTS.Upgrades;
using Gamelib.Network;
using System.IO;

namespace Facepunch.RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;
		public virtual bool HasSelectionGlow => true;

		public Dictionary<string, BaseAbility> Abilities { get; private set; }
		public Dictionary<string, IStatus> Statuses { get; private set; }
		public Dictionary<string, ItemComponent> Components { get; private set; }
		public BaseAbility UsingAbility { get; private set; }
		[Net, OnChangedCallback] public uint ItemNetworkId { get; private set; }
		[Net, Local] public List<uint> Upgrades { get; private set; }
		[Net] public Player Player { get; private set; }
		[Net] public float MaxHealth { get; set; }
		public EntityHudAnchor Hud { get; private set; }
		public EntityHudIcon StatusIcon { get; private set; }
		public EntityHudIconBar QueueHud { get; private set; }
		public Vector3 LocalCenter { get; protected set; }
		public List<QueueItem> Queue { get; private set; }
		public uint LastQueueId { get; private set; }

		public string ItemId => Item.UniqueId;
		public bool IsSelected => Tags.Has( "selected" );
		public bool IsLocalPlayers => Local.Pawn == Player;

		private T _itemCache;

		public T Item
		{
			get
			{
				if ( _itemCache == null )
					_itemCache = Items.Find<T>( ItemNetworkId );
				return _itemCache;
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
			Upgrades = new List<uint>();
			Statuses = new();
			Components = new();
			Queue = new List<QueueItem>();
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

		public bool IsOnScreen()
		{
			var position = Position.ToScreen();
			return position.x > 0f && position.y > 0f && position.x < 1f && position.y < 1f;
		}

		public bool IsInQueue( BaseItem item )
		{
			for ( var i = 0; i < Queue.Count; i++)
			{
				if ( Queue[i].Item == item )
					return true;
			}

			return false;
		}

		public bool HasUpgrade( BaseUpgrade item )
		{
			return Upgrades.Contains( item.NetworkId );
		}

		public bool HasUpgrade( uint id )
		{
			return Upgrades.Contains( id );
		}

		public bool HasComponent<C>() where C : ItemComponent
		{
			var componentName = Library.GetAttribute( typeof( C ) ).Name;
			return Components.ContainsKey( componentName );
		}

		public C GetComponent<C>() where C : ItemComponent
		{
			var componentName = Library.GetAttribute( typeof( C ) ).Name;

			if ( Components.TryGetValue( componentName, out var component ) )
				return (component as C);

			return null;
		}

		public void RemoveComponent<C>() where C : ItemComponent
		{
			var componentName = Library.GetAttribute( typeof( C ) ).Name;

			if ( Components.ContainsKey( componentName ) )
			{
				Components.Remove( componentName );
			}
		}

		public C AddComponent<C>() where  C : ItemComponent
		{
			var component = GetComponent<C>();
			if ( component != null ) return component;

			var componentName = Library.GetAttribute( typeof( C ) ).Name;
			component = Library.Create<C>( componentName );
			Components.Add( componentName, component );

			return component;
;		}

		public BaseAbility GetAbility( string id )
		{
			if ( Abilities.TryGetValue( id, out var ability ) )
				return ability;

			return null;
		}

		public bool HasStatus( string id )
		{
			return Statuses.ContainsKey( id );
		}

		public S ApplyStatus<S>( StatusData data ) where S : IStatus
		{
			Host.AssertServer();

			using var stream = new MemoryStream();
			using var writer = new BinaryWriter( stream );

			data.Serialize( writer );

			var id = Library.GetAttribute( typeof( S ) ).Name;

			ClientApplyStatus( To.Everyone, id, stream.GetBuffer() );

			if ( Statuses.TryGetValue( id, out var status ) )
			{
				status.SetData( data );
				status.Restart();

				return (S)status;
			}

			status = RTS.Statuses.Create( id );

			Statuses.Add( id, status );

			status.SetData( data );
			status.Initialize( id, this );
			status.OnApplied();

			return (S)status;
		}

		public void RemoveAllStatuses()
		{
			if ( IsServer ) ClientRemoveAllStatuses( To.Everyone );

			foreach ( var kv in Statuses )
			{
				kv.Value.OnRemoved();
			}

			Statuses.Clear();
		}

		public void RemoveStatus( string id )
		{
			if ( Statuses.TryGetValue( id, out var status ) )
			{
				if ( IsServer )
					ClientRemoveStatus( To.Everyone, id );

				Statuses.Remove( id );
				status.OnRemoved();
			}
		}

		public bool IsUsingAbility()
		{
			return (UsingAbility != null);
		}

		public virtual int GetAttackPriority()
		{
			return 0;
		}

		public virtual void StartAbility( BaseAbility ability, AbilityTargetInfo info )
		{
			CancelAbility();

			ability.LastUsedTime = 0;
			ability.NextUseTime = ability.Cooldown;
			ability.TargetInfo = info;

			ability.OnStarted();

			if ( IsServer )
			{
				ClientStartAbility( To.Single( Player ), ability.UniqueId, (Entity)info.Target, info.Origin );
			}

			UsingAbility = ability;

			if ( ability.Duration == 0f )
			{
				FinishAbility();
			}
		}

		public virtual void FinishAbility()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.OnFinished();
				UsingAbility = null;

				if ( IsServer )
				{
					ClientFinishAbility( To.Single( Player ) );
				}
			}
		}

		public virtual void CancelAbility()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.OnCancelled();
				UsingAbility = null;

				if ( IsServer )
				{
					ClientCancelAbility( To.Single( Player ) );
				}
			}
		}

		public bool IsEnemy( ISelectable other )
		{
			return (other.Player != Player);
		}

		public Vector3 GetFreePosition( UnitEntity unit, float diameterScale = 0.75f )
		{
			var bounds = GetDiameterXY( diameterScale );
			var pathfinder = unit.Pathfinder;
			var potentialNodes = new List<GridWorldPosition>();

				pathfinder.GetGridPositions( Position, bounds, potentialNodes );

			var freeLocations = potentialNodes
				.Where( v => pathfinder.IsAvailable( v ) )
				.ToList();

			if ( freeLocations.Count == 0 )
			{
				throw new Exception( "[ItemEntity::PlaceNear] Unable to find a free location to spawn the unit!" );
			}

			var randomLocation = freeLocations[Rand.Int( freeLocations.Count - 1 )];
			
			return pathfinder.GetPosition( randomLocation ) + new Vector3( 0f, 0f, pathfinder.GetHeight( randomLocation ) );
		}

		public void PlaceNear( UnitEntity unit, float diameterScale = 0.75f )
		{
			unit.Position = GetFreePosition( unit, diameterScale );
		}

		public bool IsInRange( Entity entity, float radius )
		{
			if ( entity is ModelEntity modelEntity )
			{
				// We can try to see if our range overlaps the bounding box of the target.
				var targetBounds = modelEntity.CollisionBounds + modelEntity.Position;

				if ( targetBounds.Overlaps( Position.WithZ( modelEntity.Position.z ), radius ) )
					return true;
			}

			var targetPosition = entity.Position.WithZ( 0f );
			var selfPosition = Position.WithZ( 0f );

			return (targetPosition.Distance( selfPosition ) < radius);
		}

		public void Assign( Player player, T item )
		{
			Host.AssertServer();

			Owner = player;
			Player = player;
			ItemNetworkId = item.NetworkId;

			ClearItemCache();
			OnItemChanged( item );
			OnPlayerAssigned( player );
		}

		public void ChangeTo( T item )
		{
			Host.AssertServer();

			ItemNetworkId = item.NetworkId;
			ClearItemCache();
			OnItemChanged( item );
		}

		public float GetDiameterXY( float scalar = 1f, bool smallestSide = false )
		{
			return EntityExtension.GetDiameterXY( this, scalar, smallestSide );
		}

		public void ClearItemCache() => _itemCache = null;

		public void Assign( Player player, string itemId )
		{
			Host.AssertServer();

			var item = Items.Find<T>( itemId );

			Assign( player, item );
		}

		public virtual void Select()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Add( this );
				Tags.Add( "selected" );
			}
		}

		public virtual void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
				Tags.Remove( "selected" );
			}
		}

		public virtual bool CanSelect()
		{
			return true;
		}

		public virtual bool ShouldUpdateHud()
		{
			return EnableDrawing && Hud.IsActive;
		}

		public virtual void UpdateHudComponents()
		{
			var status = Statuses.FirstOrDefault();

			if ( status.Value != null && status.Value.Icon != null )
			{
				StatusIcon.Texture = status.Value.Icon;
				StatusIcon.SetClass( "hidden", false );
			}
			else
			{
				StatusIcon.SetClass( "hidden", true );
			}

			if ( QueueHud != null && Queue.Count > 0 )
			{
				var queueItem = Queue[0];

				QueueHud.Icon.Texture = queueItem.Item.Icon;
				QueueHud.Bar.SetProgress( 1f - (queueItem.GetTimeLeft() / queueItem.Item.BuildTime) );
				QueueHud.SetActive( true );
			}
			else
			{
				QueueHud?.SetActive( false );
			}
		}

		public override void TakeDamage( DamageInfo info )
		{
			foreach ( var component in Components.Values )
				info = component.TakeDamage( info );

			base.TakeDamage( info );
		}

		public override void ClientSpawn()
		{
			Hud = EntityHud.Instance.Create( this );
			Hud.SetActive( true );

			AddHudComponents();

			base.ClientSpawn();
		}

		[Event.Tick]
		protected virtual void Tick()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.Tick();
			}

			foreach ( var kv in Statuses )
			{
				var status = kv.Value;

				if ( status.EndTime )
					RemoveStatus( status.UniqueId );
				else
					status.Tick();
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( Queue.Count > 0 )
			{
				var firstItem = Queue[0];

				if ( firstItem.FinishTime > 0f && Gamemode.Instance.ServerTime >= firstItem.FinishTime )
				{
					OnQueueItemCompleted( firstItem );
					UnqueueItem( firstItem.Id );
					firstItem.Item.OnCreated( Player, this );
				}
			}

			var ability = UsingAbility;

			if ( ability != null && ability.LastUsedTime >= ability.Duration )
			{
				FinishAbility();
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{

		}

		protected virtual void OnQueueItemCompleted( QueueItem queueItem )
		{
			if ( queueItem.Item is BaseTech tech )
			{
				Player.AddDependency( tech );
				return;
			}

			if ( queueItem.Item is BaseUpgrade upgrade )
			{
				var changeItemTo = upgrade.ChangeItemTo;

				if ( !string.IsNullOrEmpty( changeItemTo ) )
					ChangeTo( Items.Find<T>( changeItemTo ) );

				Upgrades.Add( upgrade.NetworkId );
			}
		}

		protected virtual void OnItemNetworkIdChanged()
		{
			ClearItemCache();
			CreateAbilities();
		}

		protected virtual void AddHudComponents()
		{
			StatusIcon = Hud.AddChild<EntityHudIcon>( "status" );

			if ( IsLocalPlayers )
				QueueHud = Hud.AddChild<EntityHudIconBar>();
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				RemoveAllStatuses();
				CancelAbility();
				Deselect();
			}

			if ( IsClient ) Hud.Delete();

			base.OnDestroy();
		}

		protected override void OnTagAdded( string tag )
		{
			if ( HasSelectionGlow && IsLocalPlayers && tag == "selected" )
			{
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );
			}

			base.OnTagAdded( tag );
		}

		protected override void OnTagRemoved( string tag )
		{
			if ( HasSelectionGlow && IsLocalPlayers && tag == "selected" )
			{
				GlowActive = false;
			}

			base.OnTagRemoved( tag );
		}

		protected virtual void OnPlayerAssigned( Player player) { }

		protected virtual void OnItemChanged( T item )
		{
			CreateAbilities();
		}

		protected virtual void CreateAbilities()
		{
			Abilities = new();

			foreach ( var id in Item.Abilities )
			{
				var ability = RTS.Abilities.Create( id );
				ability.Initialize( id, this );
				Abilities[id] = ability;
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
		private void RemoveFromQueue( uint queueId )
		{
			for ( var i = Queue.Count - 1; i >= 0; i-- )
			{
				if ( Queue[i].Id == queueId )
				{
					Queue.RemoveAt( i );
					RefreshSelection();
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

			RefreshSelection();
		}

		[ClientRpc]
		private void ClientRemoveAllStatuses()
		{
			RemoveAllStatuses();
		}

		[ClientRpc]
		private void ClientApplyStatus( string id, byte[] data )
		{
			using var stream = new MemoryStream( data );
			using var reader = new BinaryReader( stream );

			if ( Statuses.TryGetValue( id, out var status ) )
			{
				status.Deserialize( reader );
				status.Restart();
				return;
			}

			status = RTS.Statuses.Create( id );

			Statuses.Add( id, status );

			status.Deserialize( reader );
			status.Initialize( id, this );
			status.OnApplied();
		}

		[ClientRpc]
		private void ClientRemoveStatus( string id )
		{
			RemoveStatus( id );
		}

		[ClientRpc]
		private void ClientStartAbility( string id, Entity target, Vector3 origin )
		{
			StartAbility( GetAbility( id ), new AbilityTargetInfo()
			{
				Target = target as ISelectable,
				Origin = origin
			} );
			
			RefreshSelection();
		}

		[ClientRpc]
		private void ClientFinishAbility()
		{
			FinishAbility();
			RefreshSelection();
		}

		[ClientRpc]
		private void ClientCancelAbility()
		{
			CancelAbility();
			RefreshSelection();
		}

		private void RefreshSelection()
		{
			if ( !IsLocalPlayers || !IsSelected ) return;
			SelectedItem.Instance.Update( Player.Selection );
		}
	}
}

