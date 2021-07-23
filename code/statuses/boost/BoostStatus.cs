using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class BoostStatus : BaseStatus<BoostData>
	{
		public override string Name => "Boost";
		public override string Description => "Gotta go fast!";
		public override Texture Icon => Texture.Load( "textures/rts/statuses/boost.png" );

		private RealTimeUntil NextHealTime { get; set; }
		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				var diameter = Target.GetDiameterXY( 1.25f, false );
				Particles = Particles.Create( "particles/unit_status/boost/boost_unit.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( diameter / 2f, 0f, 1f ) );
			}

			if ( Host.IsServer && Target is UnitEntity unit )
			{
				unit.Modifiers.Speed += Data.Modifier;
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
				unit.Modifiers.Speed -= Data.Modifier;
			}
		}

		public override void Tick()
		{
			if ( Host.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
