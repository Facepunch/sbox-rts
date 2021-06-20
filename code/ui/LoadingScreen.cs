
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class LoadingScreen : Panel
	{
		public Label Text;

		public LoadingScreen()
		{
			StyleSheet.Load( "/ui/LoadingScreen.scss" );

			Text = Add.Label( "Loading", "loading" );
		}

		public override void Tick()
		{
			if ( RTS.Game == null ) return;
			
			var isHidden = (Local.Pawn is Player player && player.Camera != null);
			var round = RTS.Game.Round;

			if ( round is PlayRound )
			{
				
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
