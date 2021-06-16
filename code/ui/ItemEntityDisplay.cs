
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
			var position = (Selectable.Position + Vector3.Up * 40f).ToScreen();

			if ( position.z < 0 )
				return;

			position *= new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;

			if ( Selectable.Health <= Selectable.MaxHealth * 0.9f )
			{
				Health.Foreground.Style.Width = Length.Fraction( Selectable.Health / Selectable.MaxHealth );
				Health.SetClass( "hidden", false );
			}
			else
			{
				Health.SetClass( "hidden", true );
			}

			Style.Left = Length.Pixels( position.x );
			Style.Top = Length.Pixels( position.y - 32 );
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
