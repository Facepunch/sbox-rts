
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

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}

	[StyleSheet( "/ui/PopulationCount.scss" )]
	public class PopulationCount : Panel
	{
		public PopulationLabel Population { get; set; }

		public PopulationCount()
		{
			Population = AddChild<PopulationLabel>( "population" );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );

			if ( Game.LocalPawn is RTSPlayer player )
			{
				Population.Label.Text = player.Population + "/" + player.MaxPopulation;
				Population.Label.SetClass( "full", player.Population >= player.MaxPopulation );
			}

			base.Tick();
		}
	}
}
