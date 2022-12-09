using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class BurningStatus : BaseStatus<DamageData>
	{
		public override string Name => "Burning";
		public override string Description => "I'm on fire!";

		private RealTimeUntil NextTakeDamage { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				var radius = Target.GetDiameterXY( 0.5f, true );

				Particles = Particles.Create( "particles/weapons/flamethrower/flamethrower_fire.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( radius, 0f, 0f ) );
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
					Damage = Data.Damage
				};

				info = info.WithTag( "burn" );

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
