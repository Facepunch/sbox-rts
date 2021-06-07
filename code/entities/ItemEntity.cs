using Gamelib.Network;
using RTS.Buildings;
using Sandbox;
using Steamworks.Data;
using System;
using System.Collections.Generic;

namespace RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;

		[Net] public uint ItemId { get; set; }
		[Net] public Player Player { get; set; }

		public bool IsSelected => Tags.Has( "Selected" );

		public bool IsLocalPlayers => Player.IsValid() && Player.IsLocalPawn;

		public T Item
		{
			get
			{
				return Game.Instance.FindItem<T>( ItemId );
			}

			set
			{
				ItemId = value.NetworkId;
				OnItemChanged( value );
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
		}

		public virtual void Select()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Add( this );
				Tags.Add( "Selected" );
			}
		}

		public virtual void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
				Tags.Remove( "Selected" );
			}
		}

		protected override void OnTagAdded( string tag )
		{
			if ( IsLocalPlayers && tag == "Selected" )
			{
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );
			}

			base.OnTagAdded( tag );
		}

		protected override void OnTagRemoved( string tag )
		{
			if ( IsLocalPlayers && tag == "Selected" )
				GlowActive = false;

			base.OnTagRemoved( tag );
		}

		protected virtual void OnItemChanged( T item ) { }
	}
}

