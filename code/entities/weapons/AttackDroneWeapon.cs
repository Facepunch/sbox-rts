using Sandbox;

namespace Facepunch.RTS
{
	[Library("weapon_attack_drone")]
	public partial class AttackDroneWeapon : Weapon
	{
		public override float FireRate => 0.3f;
		public override int BaseDamage => 8;
		public override bool BoneMerge => false;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash/muzzle_large/muzzleflash_large.vpcf";
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 5f;

		public override void Attack()
		{

			base.Attack();
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( $"muzzle{Rand.Int(1, 2)}", true );
		}
	}
}
