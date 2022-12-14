using Sandbox;
using System;
using Editor;

namespace Gamelib.DayNight
{
	/// <summary>
	/// An audio controller for when a looping sound should play at each point in the day
	/// </summary>
	/// 
	[Library( "daynight_ambience" )]
	[Title( "Day and Night Ambience" )]
	[EditorSprite("editor/snd_daynight.vmat")]
	public partial class DayNightAmbience : Entity
	{
		[Property( Title = "Dawn Ambient Sound" ), FGDType( "sound" )]
		public string DawnAmbience { get; set; }
		[Property( Title = "Day Ambient Sound" ), FGDType( "sound" )]
		public string DayAmbience { get; set; }
		[Property( Title = "Dusk Ambient Sound" ), FGDType( "sound" )]
		public string DuskAmbience { get; set; }
		[Property( Title = "Night Ambient Sound" ), FGDType( "sound" )]
		public string NightAmbience { get; set; }

		public class Transition
		{
			public Sound From { get; set; }
			public Sound To { get; set; }

			public RealTimeUntil EndTime { get; private set; }
			public float Progress { get; private set; }
			public float Duration { get; private set; }
			public bool IsComplete { get; private set; }

			public Transition()
			{
				IsComplete = true;
			}

			public void Start( Sound from, Sound to, float duration )
			{
				From.Stop();

				IsComplete = false;
				Progress = 0f;
				Duration = duration;
				EndTime = duration;
				From = from;
				To = to;

				Update();
			}

			public void Update()
			{
				if ( IsComplete ) return;

				Progress = Math.Clamp( 1f - (EndTime / Duration), 0f, 1f );

				From.SetVolume( 1f - Progress );
				To.SetVolume( Progress );

				if ( Progress >= 1f )
				{
					IsComplete = true;
					From.Stop();
				}
			}
		}

		private Transition SoundTransition { get; set; }
		private Sound CurrentSound { get; set; }

		public override void Spawn()
		{
			DayNightManager.OnSectionChanged += HandleSectionChanged;
			SoundTransition = new Transition();

			base.Spawn();
		}

		private void HandleSectionChanged( TimeSection section )
		{
			if ( Game.IsClient ) return;

			if ( section == TimeSection.Dawn )
			{
				TransitionTo( DawnAmbience );
			}
			else if ( section == TimeSection.Day )
			{
				TransitionTo( DayAmbience );
			}
			else if ( section == TimeSection.Dusk )
			{
				TransitionTo( DuskAmbience );
			}
			else if ( section == TimeSection.Night )
			{
				TransitionTo( NightAmbience );
			}
		}

		private void TransitionTo( string soundName )
		{
			var sound = Sound.FromScreen( soundName );
			SoundTransition.Start( CurrentSound, sound, 5f );
			CurrentSound = sound;
		}

		[Event.Tick.Server]
		private void ServerTick()
		{
			SoundTransition?.Update();
		}
	}
}
