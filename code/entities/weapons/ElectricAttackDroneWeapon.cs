using Sandbox;

namespace Facepunch.RTS
{
	[Library( "weapon_electric_attack_drone" )]
	public partial class ElectricAttackDroneWeapon : AttackDroneWeapon
	{
		public override float FireRate => 0.7f;
		public override int BaseDamage => 10;
		public override string BulletTracer => "particles/weapons/electric_bolt/electric_bolt.vpcf";
		public override DamageFlags DamageType => DamageFlags.Shock;
		public override string SoundName => "electric.bolt2";

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
