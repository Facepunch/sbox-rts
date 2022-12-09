using Sandbox;

namespace Facepunch.RTS
{
	[Library("weapon_electric_sniper")]
	public partial class ElectricSniper : Sniper, IBallisticsWeapon
	{
		public override int BaseDamage => 30;
		public override string BulletTracer => "particles/weapons/electric_bolt/electric_bolt.vpcf";
		public override string DamageType => "shock";
		public override string SoundName => "electric.bolt1";

		public override void Attack()
		{
			if ( Target is ISelectable target && !target.HasStatus<ShockStatus>() )
			{
				target.ApplyStatus<ShockStatus>( new ModifierData()
				{
					Modifier = 2f,
					Duration = 5f
				} );
			}

			base.Attack();
		}
	}
}
