
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class CooldownLabel : Panel
	{
		public Panel Icon { get; set; }
		public Label Label { get; set; }

		public CooldownLabel()
		{
			StyleSheet.Load( "/ui/CooldownLabel.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
