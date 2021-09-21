
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class IconWithLabel : Panel
	{
		public Panel Icon { get; set; }
		public Label Label { get; set; }

		public IconWithLabel()
		{
			StyleSheet.Load( "/ui/IconWithLabel.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
