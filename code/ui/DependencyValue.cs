
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/DependencyValue.scss" )]
	public class DependencyValue : Panel
	{
		public Panel Icon { get; private set; }
		public Label Label { get; set; }

		public DependencyValue()
		{
			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void Update( BaseItem dependency )
		{
			var icon = dependency.Icon;

			if ( icon != null )
			{
				Icon.Style.BackgroundImage = icon;
				Icon.Style.BackgroundSizeX = Length.Percent( 100f );
				Icon.Style.BackgroundSizeY = Length.Percent( 100f );
			}
			else
			{
				Icon.Style.BackgroundImage = null;
				Icon.Style.BackgroundSizeX = null;
				Icon.Style.BackgroundSizeY = null;
			}

			Label.Text = dependency.Name;
		}
	}
}
