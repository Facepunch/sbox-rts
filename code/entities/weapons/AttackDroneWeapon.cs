using Sandbox;

namespace Facepunch.RTS
{
	[Library("weapon_attack_drone")]
	public partial class AttackDroneWeapon : Weapon
	{
		public override float FireRate => 0.1f;
		public override int BaseDamage => 3;
		public override bool BoneMerge => false;
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 5f;

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( $"muzzle{Game.Random.Int(1, 2)}", true );
		}
	}
}
