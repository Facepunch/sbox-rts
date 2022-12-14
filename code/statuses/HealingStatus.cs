using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class HealingStatus : BaseStatus<HealingData>
	{
		public override string Name => "Healing";
		public override string Description => "That feels good!";

		private RealTimeUntil NextHealTime { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Game.IsClient )
			{
				var diameter = Target.GetDiameterXY( 1.25f, false );
				Particles = Particles.Create( "particles/unit_status/healing/healing_unit.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( diameter / 2f, 0f, 1f ) );
			}
		}

		public override void OnRemoved()
		{
			if ( Game.IsClient )
			{
				Particles?.Destroy();
				Particles = null;
			}
		}

		public override void Tick()
		{
			if ( Game.IsServer && NextHealTime )
			{
				Target.Health = Math.Clamp( Target.Health + Data.Amount, 0f, Target.MaxHealth );
				NextHealTime = Data.Interval;
			}
			else if ( Game.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
