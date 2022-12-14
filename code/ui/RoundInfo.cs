
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/RoundInfo.scss" )]
	public class RoundInfo : Panel
	{
		public Panel Container;
		public Label RoundName;

		public RoundInfo()
		{
			Container = Add.Panel( "container" );
			RoundName = Container.Add.Label( "Round", "roundName" );
		}

		public override void Tick()
		{
			var round = Rounds.Current;
			var showRoundInfo = round?.ShowRoundInfo ?? false;

			SetClass( "hidden", !Hud.IsLocalPlaying() || !showRoundInfo );

			if ( showRoundInfo )
				RoundName.Text = round.RoundName;
		}
	}
}
