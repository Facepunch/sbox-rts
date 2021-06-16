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

		[Net] public uint ItemId { get; set; }
		[Net] public Player Player { get; private set; }
		[Net] public float MaxHealth { get; set; }

		public bool IsSelected => Tags.Has( "selected" );

		public bool IsLocalPlayers => Player.IsValid() && Player.IsLocalPawn;

		public T Item
		{
			get
			{
				return ItemManager.Instance.Find<T>( ItemId );
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
		}

		public void Assign( Player player, T item )
		{
			Host.AssertServer();

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

		protected override void OnTagAdded( string tag )
		{
			if ( IsLocalPlayers && tag == "selected" )
			{
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );
			}

			base.OnTagAdded( tag );
		}

		protected override void OnTagRemoved( string tag )
		{
			if ( IsLocalPlayers && tag == "selected" )
			{
				GlowActive = false;
			}

			base.OnTagRemoved( tag );
		}

		protected virtual void OnPlayerAssigned( Player player) { }

		protected virtual void OnItemChanged( T item ) { }
	}
}

