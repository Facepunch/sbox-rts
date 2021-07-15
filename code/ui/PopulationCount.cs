
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
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
			SetClass( "hidden", !Hud.IsLocalPlaying() );

			if ( Local.Pawn is Player player )
			{
				Population.Label.Text = player.Population + "/" + player.MaxPopulation;
				Population.Label.SetClass( "full", player.Population >= player.MaxPopulation );
			}

			base.Tick();
		}
	}
}
