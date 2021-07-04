using Sandbox;

namespace Gamelib.DayNight
{
	public static partial class DayNightManager
	{
		public static float TimeOfDay { get; set; } = 0f;
		public static float Speed { get; set; } = 1f;

		[Event.Tick.Server]
		private static void Tick()
		{
			TimeOfDay += Speed * Time.Delta;

			if ( TimeOfDay >= 24f )
			{
				TimeOfDay = 0f;
			}
		}
	}
}
