using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	public class FrenzyStatus : BaseStatus<ModifierData>
	{
		public override string Name => "Frenzy";
		public override string Description => "I can't stop shooting!";

		private Particles Particles { get; set; }
		private float FireRateDelta { get; set; }

		public override void OnApplied()
		{
			if ( Host.IsClient )
			{
				var diameter = Target.GetDiameterXY( 1.25f, false );
				Particles = Particles.Create( "particles/unit_status/damage/damage_unit.vpcf" );
				Particles.SetPosition( 0, Target.Position );
				Particles.SetPosition( 1, new Vector3( diameter / 2f, 0f, 1f ) );
			}

			if ( Host.IsServer && Target is UnitEntity unit )
			{
				var oldValue = unit.Modifiers.FireRate;
				unit.Modifiers.FireRate *= Data.Modifier;
				FireRateDelta = oldValue - unit.Modifiers.FireRate;
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
				unit.Modifiers.FireRate += FireRateDelta;
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
