using Sandbox;

namespace Gamelib.DayNight
{

	/// <summary>
	/// A way to play sounds during the day night cycle, this also allows you to play a looping sound between 2 time periods e.g. 5pm to 6pm. You can also set the sound to only place once.
	/// </summary>

	[Library( "daynight_sound" )]
	[Hammer.EntityTool( "Sound", "Day and Night" )]
	[Hammer.EditorSprite("editor/daynight_sound.vmat")]
	public partial class DayNightSound : Entity
	{
		[Property( Title = "Sound To Play" ), FGDType( "sound" )]
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
