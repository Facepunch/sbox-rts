using Sandbox;
using Sandbox.UI;

namespace Gamelib.UI
{
	public static class UIUtility
	{
		public static Panel GetHoveredPanel( Panel root = null )
		{
			root ??= Local.Hud;

			if ( root.PseudoClass.HasFlag( PseudoClass.Hover ) )
			{
				if ( root.ComputedStyle.PointerEvents.HasValue )
				{
					if ( root.ComputedStyle.PointerEvents == PointerEvents.All )
						return root;
				}
			}

			foreach ( var child in root.Children )
			{
				var panel = GetHoveredPanel( child );

				if ( panel != null )
					return panel;
			}

			return null;
		}
	}
}
