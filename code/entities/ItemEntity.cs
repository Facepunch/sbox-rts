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

		public bool IsSelected => Tags.Has( "selected" );

		public bool IsLocalPlayers => Player.IsValid() && Player.IsLocalPawn;

		public T Item
		{
			get
			{
				return Game.Instance.FindItem<T>( ItemId );
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
		}

		public void Assign( Player player, string itemId )
		{
			Host.AssertServer();

			var item = Game.Instance.FindItem<T>( itemId );

			Player = player;
			ItemId = item.NetworkId;

			OnItemChanged( item );
			OnPlayerAssigned( player );
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

		protected override void OnTagAdded( string tag )
		{
			if ( IsLocalPlayers && tag == "selected" )
			{
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );

				SelectedItem.Instance.Update( Player.Selection.Select( i => (i as ISelectable) ).ToList() );
			}

			base.OnTagAdded( tag );
		}

		protected override void OnTagRemoved( string tag )
		{
			if ( IsLocalPlayers && tag == "selected" )
			{
				GlowActive = false;
				SelectedItem.Instance.Update( Player.Selection.Select( i => (i as ISelectable) ).ToList() );
			}

			base.OnTagRemoved( tag );
		}

		protected virtual void OnPlayerAssigned( Player player) { }

		protected virtual void OnItemChanged( T item ) { }
	}
}

