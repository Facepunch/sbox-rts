using Facepunch.RTS;
using Sandbox;
using System.Threading.Tasks;

namespace Gamelib.DayNight
{
	[Library( "snd_daynight" )]
	[Hammer.EditorSprite("editor/snd_daynight.vmat")]
	public partial class DayNightAudio : Entity
	{
		[Property(	FGDType = "sound", Title = "Sound For Day" )]
		public string DayAmbient { get; set; }

		[Property(	FGDType = "sound", Title = "Sound For Night" )]
		public string NightAmbient { get; set; }

		[Property(	FGDType = "sound", Title = "Sound For Day Start")]
		public string DayStart { get; set; }

		[Property(	FGDType = "sound", Title = "Sound For Night Start")]
		public string NightStart { get; set; }


		private Sound NightSound { get; set; }
		private Sound DaySound { get; set; }



		public override void Spawn()
		{
			base.Spawn();
			DayNightManager.OnSectionChanged += HandleSectionChanged;

			_ = PlayDaySoundAsync( 2f );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

		}

		private TimeSection CurrentSection;

		public async Task PlayDaySoundAsync( float delay = 0 )
		{
			await GameTask.DelaySeconds( delay );

			if ( DayNightManager.TimeOfDay > 0f && DayNightManager.TimeOfDay <= 18f )
			{
				DaySound = PlaySound( DayAmbient );
			}
			else if ( DayNightManager.TimeOfDay > 18f && DayNightManager.TimeOfDay <= 24f )
			{
				NightSound = PlaySound( NightAmbient );
			}

		}

		private void HandleSectionChanged( TimeSection section )
		{
			CurrentSection = section;

			if ( section == TimeSection.Dawn )
			{
				NightSound.Stop();
				DaySound = PlaySound(DayAmbient);

				PlaySound( DayStart );
			}

			else if ( section == TimeSection.Dusk )
			{
				DaySound.Stop();
				NightSound = PlaySound(NightAmbient);

				PlaySound( NightStart );
			}
		}
	}
}
