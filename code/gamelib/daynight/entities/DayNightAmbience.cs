using Sandbox;

namespace Gamelib.DayNight
{
	[Library( "daynight_ambience" )]
	[Hammer.EntityTool( "Day Night Ambience", "Day Night System" )]
	[Hammer.EditorSprite("editor/snd_daynight.vmat")]
	public partial class DayNightAmbience : Entity
	{
		[Property( FGDType = "sound", Title = "Dawn Ambient Sound" )]
		public string DawnAmbience { get; set; }
		[Property(	FGDType = "sound", Title = "Day Ambient Sound" )]
		public string DayAmbience { get; set; }
		[Property( FGDType = "sound", Title = "Dusk Ambient Sound" )]
		public string DuskAmbience { get; set; }

		[Property(	FGDType = "sound", Title = "Night Ambient Sound" )]
		public string NightAmbience { get; set; }

		private Sound CurrentSound { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			DayNightManager.OnSectionChanged += HandleSectionChanged;
		}

		private void HandleSectionChanged( TimeSection section )
		{
			CurrentSound.Stop();

			if ( section == TimeSection.Dawn )
			{
				CurrentSound = PlaySound( DawnAmbience );
			}
			else if ( section == TimeSection.Day )
			{
				CurrentSound = PlaySound( DayAmbience );
			}
			else if ( section == TimeSection.Dusk )
			{
				CurrentSound = PlaySound( DuskAmbience );
			}
			else if ( section == TimeSection.Night )
			{
				CurrentSound = PlaySound( NightAmbience );
			}
		}
	}
}
