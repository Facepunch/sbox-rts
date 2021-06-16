
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace RTS
{
	public class ItemEntityHealth : Panel
	{
		public Panel Foreground { get; private set; }

		public ItemEntityHealth()
		{
			Foreground = Add.Panel( "foreground" );
		}

		public override void Tick()
		{

		}
	}

	public class ItemEntityContainer : Panel
	{
		public ItemEntityHealth Health { get; private set; }
		public ISelectable Selectable { get; private set; }

		public ItemEntityContainer()
		{
			Health = AddChild<ItemEntityHealth>();
		}

		public void SetEntity( ISelectable selectable )
		{
			Selectable = selectable;
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void Update()
		{
			var position = Selectable.Position.ToScreen();

			if ( position.z < 0 )
				return;

			Health.Foreground.Style.Width = Length.Fraction( Selectable.Health / Selectable.MaxHealth );

			Log.Info( position.ToString() );

			Style.Left = Length.Fraction( position.x );
			Style.Top = Length.Fraction( position.y );
			Style.Dirty();
		}
	}

	public class ItemEntityDisplay : Panel
	{
		public static ItemEntityDisplay Instance { get; private set; }

		public ItemEntityDisplay()
		{
			StyleSheet.Load( "/ui/ItemEntityDisplay.scss" );

			Instance = this;
		}

		public ItemEntityContainer Create( ISelectable selectable )
		{
			var container = AddChild<ItemEntityContainer>();
			container.SetEntity( selectable );
			return container;
		}
	}
}
