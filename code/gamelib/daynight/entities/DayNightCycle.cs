using Sandbox;
using System;
using System.Linq;

namespace Gamelib.DayNight
{
	[Library( "day_night_cycle" )]
	[Hammer.EntityTool( "Day Night Cycle", "Lightning" )]
    public class DayNightCycle : ModelEntity
	{
		//[Property( "DawnColor", Title = "Dawn Color" )]
		public Color DawnColor { get; set; } = Color.Yellow;

		//[Property( "DayColor", Title = "Day Color" )]
		public Color DayColor { get; set; } = Color.White;

		//[Property( "DuskColor", Title = "Dusk Color" )]
		public Color DuskColor { get; set; } = Color.Orange;

		//[Property( "NightColor", Title = "Night Color" )]
		public Color NightColor { get; set; } = Color.Blue;

		public EnvironmentLightEntity Environment
		{
			get
			{
				if ( _environment == null )
					_environment = All.OfType<EnvironmentLightEntity>().FirstOrDefault();
				return _environment;
			}
		}

		private EnvironmentLightEntity _environment;

		[Event.Tick.Client]
		private void ClientTick()
		{
			var environment = Environment;
			if ( environment == null ) return;

			var manager = DayNightManager.Instance;
			if ( manager == null ) return;

			Color targetColor;

			if ( manager.TimeOfDay > 7f && manager.TimeOfDay < 10f )
			{
				targetColor = DawnColor;
			}
			else if ( manager.TimeOfDay >= 10f && manager.TimeOfDay < 18f )
			{
				targetColor = DayColor;
			}
			else if ( manager.TimeOfDay >= 18f && manager.TimeOfDay <= 21f )
			{
				targetColor =  DuskColor;
			}
			else
			{
				targetColor = NightColor;
			}

			//environment.Color = Color.Lerp( environment.Color, targetColor, ( 1f / 24f ) * manager.Speed * Time.Delta );
		}
	}
}
