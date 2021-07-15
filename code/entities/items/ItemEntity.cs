using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.Extensions;
using Gamelib.FlowFields.Grid;
using Facepunch.RTS;

namespace Facepunch.RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;
		public virtual bool HasSelectionGlow => true;
		public virtual int AttackPriority => 0;

		public Dictionary<string, BaseAbility> Abilities { get; private set; }
		public Dictionary<string, BaseStatus> Statuses { get; private set; }
		public Dictionary<string, ItemComponent> Components { get; private set; }
		public BaseAbility UsingAbility { get; private set; }
		[Net, OnChangedCallback] public uint ItemNetworkId { get; set; }
		[Net] public Player Player { get; private set; }
		[Net] public float MaxHealth { get; set; }
		public EntityHudAnchor Hud { get; private set; }
		public EntityHudIcon StatusIcon { get; private set; }
		public Vector3 LocalCenter { get; protected set; }

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
			Statuses = new();
			Components = new();
		}

		public bool IsOnScreen()
		{
			var position = Position.ToScreen();
			return position.x > 0f && position.y > 0f && position.x < 1f && position.y < 1f;
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

		public BaseStatus ApplyStatus( string id )
		{
			if ( IsServer ) ClientApplyStatus( To.Everyone, id );

			if ( Statuses.TryGetValue( id, out var status ) )
			{
				status.Restart();
				return status;
			}

			status = RTS.Statuses.Create( id );

			Statuses.Add( id, status );

			status.Initialize( id, this );
			status.OnApplied();

			return status;
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

			unit.Pathfinder.GetGridPositions( Position, bounds, potentialNodes );

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

		public void Assign( Player player, T item )
		{
			Host.AssertServer();

			Owner = player;
			Player = player;
			ItemNetworkId = item.NetworkId;

			OnItemChanged( item );
			OnPlayerAssigned( player );
		}

		public float GetDiameterXY( float scalar = 1f, bool smallestSide = false )
		{
			return EntityExtension.GetDiameterXY( this, scalar, smallestSide );
		}

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

		protected virtual void OnItemNetworkIdChanged()
		{
			CreateAbilities();
		}

		protected virtual void AddHudComponents()
		{
			StatusIcon = Hud.AddChild<EntityHudIcon>( "status" );
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
		private void ClientRemoveAllStatuses()
		{
			RemoveAllStatuses();
		}

		[ClientRpc]
		private void ClientApplyStatus( string id )
		{
			ApplyStatus( id );
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
			FinishAbility(); RefreshSelection();
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

