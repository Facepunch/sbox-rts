
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/LoadingScreen.scss" )]
	public class LoadingScreen : Panel
	{
		public Label Text;

		public LoadingScreen()
		{
			Text = Add.Label( "Loading", "loading" );
		}

		public override void Tick()
		{
			var isHidden = Game.LocalPawn.IsValid();

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
