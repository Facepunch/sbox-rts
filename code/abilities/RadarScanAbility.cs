using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_radar_scan" )]
	public class RadarScanAbility : BaseAbility
	{
		public override string Name => "Radar Scan";
		public override string Description => "Reveal an area for a short period of time to gain intel.";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 60f;
		public override float Duration => 8f;
		public override float MaxDistance => 6000f;
		public override float AreaOfEffectRadius => 1000f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			
		};

		private Particles Effect { get; set; }
		private Sound Sound { get; set; }

		public override void OnStarted()
		{
			if ( Host.IsClient )
			{
				if ( User.IsLocalPlayers )
				{
					Fog.AddTimedViewer( TargetInfo.Origin, AreaOfEffectRadius, Duration );

					Effect = Particles.Create( "particles/radar_scan/radar_scan.vpcf" );
					Effect.SetPosition( 0, TargetInfo.Origin.WithZ( 5f ) );
					Effect.SetPosition( 1, new Vector3( AreaOfEffectRadius, 0f, 0f ) );
					Effect.SetPosition( 2, User.Player.TeamColor * 255f );

					Sound = Sound.FromWorld( "rts.scanlong", TargetInfo.Origin );
				}
			}

			base.OnStarted();
		}

		public override void OnFinished()
		{
			Reset();

			base.OnFinished();
		}

		public override void OnCancelled()
		{
			Reset();

			base.OnCancelled();
		}

		private void Reset()
		{
			Effect?.Destroy();
			Effect = null;

			Sound.Stop();
		}
	}
}
