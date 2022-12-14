
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/IconWithLabel.scss" )]
	public class IconWithLabel : Panel
	{
		public Panel Icon { get; set; }
		public Label Label { get; set; }

		public IconWithLabel()
		{
			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
