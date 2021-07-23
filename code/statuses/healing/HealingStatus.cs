using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class HealingStatus : BaseStatus<HealingData>
	{
		public override string Name => "Healing";
		public override string Description => "Damn, that feels good!";

		private RealTimeUntil NextHealTime { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				var diameter = Target.GetDiameterXY( 1.25f, false );
				Particles = Particles.Create( "particles/unit_status/healing/healing_unit.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( diameter / 2f, 0f, 1f ) );
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
			if ( Host.IsServer && NextHealTime )
			{
				Target.Health = Math.Clamp( Target.Health + Data.Amount, 0f, Target.MaxHealth );
				NextHealTime = Data.Interval;
			}
			else if ( Host.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
