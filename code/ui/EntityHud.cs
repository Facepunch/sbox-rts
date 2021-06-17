
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace RTS
{
	public class EntityHudBar : Panel
	{
		public Panel Foreground { get; private set; }

		public EntityHudBar()
		{
			Foreground = Add.Panel( "foreground" );
		}
	}

	public class EntityHudAnchor : Panel
	{
		public Entity Entity { get; private set; }

		public void SetEntity( Entity entity)
		{
			Entity = entity;
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public void UpdatePosition()
		{
			var position = (Entity.Position + Vector3.Up * 40f).ToScreen();
			if ( position.z < 0 ) return;

			position *= new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;

			Style.Left = Length.Pixels( position.x );
			Style.Top = Length.Pixels( position.y - 32 );
			Style.Dirty();
		}
	}

	public class EntityHud : Panel
	{
		public static EntityHud Instance { get; private set; }

		public EntityHud()
		{
			StyleSheet.Load( "/ui/EntityHud.scss" );

			Instance = this;
		}

		public EntityHudAnchor Create( Entity entity )
		{
			var container = AddChild<EntityHudAnchor>();
			container.SetEntity( entity );
			return container;
		}
	}
}
