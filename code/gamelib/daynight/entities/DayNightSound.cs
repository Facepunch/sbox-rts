using Sandbox;

namespace Gamelib.DayNight
{
	[Library( "daynight_sound" )]
	[Hammer.EntityTool( "Day Night Sound", "Day Night System" )]
	[Hammer.EditorSprite("editor/snd_daynight.vmat")]
	public partial class DayNightSound : Entity
	{
		[Property( FGDType = "sound", Title = "Sound To Play" )]
		public string SoundToPlay { get; set; }
		[Property( Title = "Time To Play" )]
		public int TimeToPlay { get; set; } = 12;
		[Property( Title = "Time To Stop" )]
		public int TimeToStop { get; set; } = -1;
		[Property( Title = "Delete On Play" )]
		public bool DeleteOnPlay { get; set; } = false;

		private Sound CurrentSound { get; set; }
		private float LastTime { get; set; }

		[Event.Tick.Server]
		private void ServerTick()
		{
			var currentTime = DayNightManager.TimeOfDay;

			if ( TimeToStop >= 0 && LastTime < TimeToStop && currentTime >= TimeToStop )
			{
				CurrentSound.Stop();
			}
			else if ( LastTime < TimeToPlay && currentTime >= TimeToPlay )
			{
				CurrentSound.Stop();
				CurrentSound = PlaySound( SoundToPlay );

				if ( DeleteOnPlay )
				{
					Delete();
				}
			}

			LastTime = currentTime;
		}
	}
}
