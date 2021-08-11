
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

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void Update( BaseItem dependency )
		{
			var icon = dependency.Icon; ;

			if ( icon != null )
			{
				Icon.Style.Background = new PanelBackground
				{
					SizeX = Length.Percent( 100f ),
					SizeY = Length.Percent( 100f ),
					Texture = icon
				};

				Icon.Style.Dirty();
			}

			Label.Text = dependency.Name;
		}
	}
}
