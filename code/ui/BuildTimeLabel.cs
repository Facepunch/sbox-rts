
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class BuildTimeLabel : Panel
	{
		public Panel Icon { get; set; }
		public Label Label { get; set; }

		public BuildTimeLabel()
		{
			StyleSheet.Load( "/ui/BuildTimeLabel.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
