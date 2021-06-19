
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace RTS
{
	public class PopulationLabel : Panel
	{
		public Panel Icon { get; set; }
		public Label Label { get; set; }

		public PopulationLabel()
		{
			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}
	}

	public class PopulationCount : Panel
	{
		public PopulationLabel Population { get; set; }

		public PopulationCount()
		{
			StyleSheet.Load( "/ui/PopulationCount.scss" );

			Population = AddChild<PopulationLabel>( "population" );
		}

		public override void Tick()
		{
			SetClass( "hidden", true);

			var player = Local.Pawn as Player;
			if ( player == null || player.IsSpectator ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			if ( round is PlayRound )
				SetClass( "hidden", false );

			Population.Label.Text = player.Population + "/" + player.MaxPopulation;
			Population.Label.SetClass( "full", player.Population >= player.MaxPopulation );

			base.Tick();
		}
	}
}
