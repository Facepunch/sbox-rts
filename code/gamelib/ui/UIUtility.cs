using Sandbox;
using Sandbox.UI;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Gamelib.UI
{
	public static class UIUtility
	{
		public static Panel GetHoveredPanel( Panel root = null )
		{
			root ??= Local.Hud;

			if ( root.PseudoClass.HasFlag( PseudoClass.Hover ) )
			{
				if ( !string.IsNullOrEmpty( root.ComputedStyle.PointerEvents ) )
				{
					if ( root.ComputedStyle.PointerEvents != "visible" && root.ComputedStyle.PointerEvents != "none" )
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
