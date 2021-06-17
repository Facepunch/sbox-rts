using Gamelib.Network;
using RTS.Buildings;
using Sandbox;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;
		public virtual bool HasSelectionGlow => true;

		[Net] public uint ItemId { get; set; }
		[Net] public Player Player { get; private set; }
		[Net] public float MaxHealth { get; set; }
		public EntityHudAnchor UI { get; private set; }

		public bool IsSelected => Tags.Has( "selected" );
		public bool IsLocalPlayers => Player.IsValid() && Player.IsLocalPawn;

		private T _itemCache;

		public T Item
		{
			get
			{
				if ( _itemCache == null )
					_itemCache = ItemManager.Instance.Find<T>( ItemId );
				return _itemCache;
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
		}

		public void Assign( Player player, T item )
		{
			Host.AssertServer();

			Owner = player;
			Player = player;
			ItemId = item.NetworkId;

			OnItemChanged( item );
			OnPlayerAssigned( player );
		}

		public void Assign( Player player, string itemId )
		{
			Host.AssertServer();

			var item = ItemManager.Instance.Find<T>( itemId );

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

		public override void ClientSpawn()
		{
			UI = EntityHud.Instance.Create( this );

			AddHudComponents();

			base.ClientSpawn();
		}

		[Event.Frame]
		protected void UpdateHudAnchor()
		{
			if ( IsClient && ShouldUpdateHud() )
			{
				UpdateHudComponents();
				UI.UpdatePosition();
			}
		}

		protected virtual void AddHudComponents() { }

		protected virtual void UpdateHudComponents() { }

		protected virtual bool ShouldUpdateHud()
		{
			return EnableDrawing;
		}

		protected override void OnDestroy()
		{
			if ( IsClient ) UI.Delete();

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

		protected virtual void OnItemChanged( T item ) { }
	}
}

