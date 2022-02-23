
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class DependencyValue : Panel
	{
		public Panel Icon { get; private set; }
		public Label Label { get; set; }

		public DependencyValue()
		{
			StyleSheet.Load( "/ui/DependencyValue.scss" );

			//Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void Update( BaseItem dependency )
		{
			//var icon = dependency.Icon;

			//if ( icon != null )
			//{
			//	Style.BackgroundImage = icon;
			//	Style.BackgroundSizeX = Length.Percent( 100f );
			//	Style.BackgroundSizeY = Length.Percent( 100f );
			//}
			//else
			//{
			//	Style.BackgroundImage = null;
			//	Style.BackgroundSizeX = null;
			//	Style.BackgroundSizeY = null;
			//}

			Label.Text = dependency.Name;
		}
	}
}
