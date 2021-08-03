using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class FreezingStatus : BaseStatus<FreezingData>
	{
		public override string Name => "Freezing";
		public override string Description => "I can barely move!";

		private RealTimeUntil NextTakeDamage { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				Particles = Particles.Create( "particles/weapons/flamethrower/flamethrower_fire.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( 1f, 0f, 0f ) );
			}

			if ( Host.IsServer && Target is UnitEntity unit )
			{
				unit.Modifiers.Speed -= Data.SpeedReduction;
			}
		}

		public override void OnRemoved()
		{
			if ( Host.IsClient )
			{
				Particles?.Destroy();
				Particles = null;
			}

			if ( Host.IsServer && Target is UnitEntity unit )
			{
				unit.Modifiers.Speed += Data.SpeedReduction;
			}
		}

		public override void Tick()
		{
			if ( Host.IsServer && NextTakeDamage )
			{
				var info = new DamageInfo
				{
					Flags = DamageFlags.BlastWaterSurface,
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
