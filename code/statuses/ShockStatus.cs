using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class ShockStatus : BaseStatus<ModifierData>
	{
		public override string Name => "Shock";
		public override string Description => "I can barely function!";

		private Particles Particles { get; set; }
		private float FireRateDelta { get; set; }

		public override void OnApplied()
		{
			if ( Game.IsClient )
			{
				var radius = Target.GetDiameterXY( 0.5f, true );

				Particles = Particles.Create( "particles/weapons/emp_ground/emp_ground_emp.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( radius, 0f, 0f ) );
			}

			if ( Game.IsServer && Target is UnitEntity unit )
			{
				var oldValue = unit.Modifiers.FireRate;
				unit.Modifiers.FireRate *= Data.Modifier;
				FireRateDelta = oldValue - unit.Modifiers.FireRate;
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
				unit.Modifiers.FireRate += FireRateDelta;
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
