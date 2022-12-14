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
			if ( Game.IsClient )
			{
				var radius = Target.GetDiameterXY( 0.5f, true );

				Particles = Particles.Create( "particles/weapons/ice_ground/ice_ground_ice.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( radius, 0f, 0f ) );
			}

			if ( Game.IsServer && Target is UnitEntity unit )
			{
				unit.Modifiers.Speed -= Data.SpeedReduction;
			}
		}

		public override void OnRemoved()
		{
			if ( Game.IsClient )
			{
				Particles?.Destroy();
				Particles = null;
			}

			if ( Game.IsServer && Target is UnitEntity unit )
			{
				unit.Modifiers.Speed += Data.SpeedReduction;
			}
		}

		public override void Tick()
		{
			if ( Game.IsServer && NextTakeDamage )
			{
				var info = new DamageInfo
				{
					Damage = Data.Damage
				};

				info = info.WithTag( "cold" );

				Target.TakeDamage( info );
				NextTakeDamage = Data.Interval;
			}
			else if ( Game.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
