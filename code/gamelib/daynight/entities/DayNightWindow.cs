using Sandbox;
using System.Threading.Tasks;

namespace Gamelib.DayNight
{
	/// <summary>
	/// A brush entity that will enable and disable at random times during the morning and night time.
	/// </summary>

	[Library( "daynight_window" )]
	[Hammer.EntityTool( "Day Night Window Brush", "Day and Night" )]
	[Hammer.Solid]
	public class DayNightWindow : FuncBrush
	{
		[Property( Title = "Enable Delay Time" )]
		public float EnableDelay { get; set; } = 3f;
		[Property( Title = "Disable Delay Time" )]
		public  float DisableDelay { get; set; } = 3f;

		public override void Spawn()
		{
			base.Spawn();

			DayNightManager.OnSectionChanged += HandleSectionChanged;
		}

		private void HandleSectionChanged( TimeSection section )
		{
			if ( section == TimeSection.Dawn )
			{
				_ = DisableAsync( Rand.Float( DisableDelay ) );
			}
			else if ( section == TimeSection.Dusk )
			{
				_ = EnableAsync( Rand.Float( EnableDelay ) );
			}
		}

		private async Task EnableAsync( float delay )
		{
			await GameTask.DelaySeconds(delay );
			Enable();
		}

		private async Task DisableAsync( float delay )
		{
			await GameTask.DelaySeconds( delay );
			Disable();
		}
	}
}
