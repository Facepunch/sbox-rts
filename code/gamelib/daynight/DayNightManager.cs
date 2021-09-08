using Sandbox;

namespace Gamelib.DayNight
{
	public static partial class DayNightManager
	{
		public delegate void SectionChanged( TimeSection section );
		public static event SectionChanged OnSectionChanged;

		public static TimeSection Section { get; private set; }
		public static float TimeOfDay { get; set; } = 9f;
		public static float Speed { get; set; } = 0.05f;

		private static RealTimeUntil NextUpdate { get; set; }
		private static bool Initialized { get; set; }

		public static TimeSection ToSection( float time )
		{
			if ( time > 5f && time <= 9f )
				return TimeSection.Dawn;

			if ( time > 9f && time <= 18f )
				return TimeSection.Day;

			if ( time > 18f && time <= 21f )
				return TimeSection.Dusk;

			return TimeSection.Night;
		}

		[Event.Tick.Server]
		private static void Tick()
		{
			TimeOfDay += Speed * Time.Delta;

			if ( TimeOfDay >= 24f )
			{
				TimeOfDay = 0f;
			}

			var currentSection = ToSection( TimeOfDay );

			if ( currentSection != Section )
			{
				Section = currentSection;
				OnSectionChanged?.Invoke( currentSection );
			}

			if ( NextUpdate )
			{
				ChangeSectionForClient( To.Everyone, Section );
				NextUpdate = 1f;
			}
		}

		[ClientRpc]
		public static void ChangeSectionForClient( TimeSection section )
		{
			Host.AssertClient();

			if ( !Initialized || Section != section )
			{
				Section = section;
				Initialized = true;
				OnSectionChanged?.Invoke( section );
			}
		}
	}
}
