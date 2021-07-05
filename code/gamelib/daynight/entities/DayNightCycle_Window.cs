using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gamelib.DayNight
{
	[Library("func_daynightwindow")]
	[Hammer.Solid]

	public class FuncDayNightWindow : FuncBrush
	{
		//Hammer Properties.
		[Property("WindowDelayEnable", Title = "Window Enable Delay Time")]
		public float EnableDelay { get; set; } = 3;
		[Property("WindowDelayDisable", Title = "Window Disable Delay Time")]
		public  float DisableDelay { get; set; } = 3;

		//Piggy back from Conna's code
		public override void Spawn()
		{
			DayNightManager.OnSectionChanged += HandleSectionChanged;

			base.Spawn();
			Transmit = TransmitType.Always;
		}

		//Calls Enable and Disable.
		private void HandleSectionChanged(TimeSection section)
		{
			Random random = new();
			if (section == TimeSection.Dawn)
			{
				_ = DisableAsync((float)(random.NextDouble() * DisableDelay));
			}
			else if (section == TimeSection.Dusk)
			{
				_ = EnableAsync((float)(random.NextDouble() * EnableDelay));
			}
		}

		//Delays Enable/ Disable.
		private async Task EnableAsync(float delay)
		{
			await GameTask.DelaySeconds(delay);
			Enable();
		}

		private async Task DisableAsync(float delay)
		{
			await GameTask.DelaySeconds(delay);
			Disable();
		}
	}
}
