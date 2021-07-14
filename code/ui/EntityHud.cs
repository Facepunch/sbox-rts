
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class EntityHudIconList : Panel
	{

	}

	public class EntityHudIcon : Image
	{

	}

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
		public bool IsActive { get; private set; }

		public void SetEntity( Entity entity)
		{
			Entity = entity;
		}

		public void SetActive( bool active )
		{
			if ( IsActive != active )
			{
				IsActive = active;
				SetClass( "hidden", !active );
				if ( active ) UpdatePosition();
			}
		}

		public void UpdatePosition()
		{
			var position = (Entity.Position + Vector3.Up * 40f).ToScreen();
			if ( position.z < 0 ) return;

			position *= new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;

			Style.Left = Length.Pixels( position.x );
			Style.Top = Length.Pixels( position.y - 32f );
			Style.Dirty();
		}

		public override void Tick()
		{
			if ( Entity is ISelectable selectable )
			{
				if ( selectable.ShouldUpdateHud() )
				{
					selectable.UpdateHudComponents();
					UpdatePosition();
				}
			}
			else
            {
				UpdatePosition();
            }

			base.Tick();
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
