using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class WeakStatus : BaseStatus<ModifierData>
	{
		public override string Name => "Weak";
		public override string Description => "I feel so weak!";

		private Particles Particles { get; set; }

		public override void OnApplied()
		{
			if ( Game.IsClient )
			{
				var radius = Target.GetDiameterXY( 0.5f, true );

				Particles = Particles.Create( "particles/weapons/plasma_ground/plasma_ground_fire.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( radius, 0f, 0f ) );
			}

			if ( Game.IsServer )
			{
				if ( Target is UnitEntity unit )
				{
					unit.AddResistance( "resistance.bullet", -Data.Modifier );
					unit.AddResistance( "resistance.explosive", -Data.Modifier );
				}
			}
		}

		public override void OnRemoved()
		{
			if ( Game.IsClient )
			{
				Particles?.Destroy();
				Particles = null;
			}

			if ( Game.IsServer )
			{
				if ( Target is UnitEntity unit )
				{
					unit.AddResistance( "resistance.bullet", Data.Modifier );
					unit.AddResistance( "resistance.explosive", Data.Modifier );
				}
			}
		}

		public override void Tick()
		{
			if ( Game.IsClient )
			{
				Particles.SetPosition( 0, Target.Position );
			}
		}
	}
}
