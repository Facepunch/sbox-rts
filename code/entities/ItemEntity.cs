using RTS.Buildings;
using Sandbox;
using Steamworks.Data;

namespace RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;

		[Net] public uint ItemId { get; set; }
		[Net] public bool IsSelected { get; set; }
		[Net] public Player Player { get; set; }

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
				IsSelected = true;
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );
			}
		}

		public virtual void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
				IsSelected = false;
				GlowActive = false;
			}
		}

		public virtual void Highlight()
		{
			throw new System.NotImplementedException();
		}

		public virtual void Unhighlight()
		{
			throw new System.NotImplementedException();
		}

		protected virtual void OnItemChanged( T item ) { }
	}
}

