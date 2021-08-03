using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class DissolvingStatus : BaseStatus<DamageData>
	{
		public override string Name => "Dissolving";
		public override string Description => "My skin is burning!";

		private RealTimeUntil NextTakeDamage { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				Particles = Particles.Create( "particles/weapons/plasma_ground/plasma_ground_fire.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( 1f, 0f, 0f ) );
			}
		}

		public override void OnRemoved()
		{
			if ( Host.IsClient )
			{
				Particles?.Destroy();
				Particles = null;
			}
		}

		public override void Tick()
		{
			if ( Host.IsServer && NextTakeDamage )
			{
				var info = new DamageInfo
				{
					Flags = DamageFlags.Plasma,
					Damage = Data.Damage
				};

				Target.TakeDamage( info );
				NextTakeDamage = Data.Interval;
			}
			else if ( Host.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
