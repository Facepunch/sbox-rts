using Sandbox;

namespace Facepunch.RTS
{
	[Library("weapon_suicide_drone")]
	public partial class SuicideDroneWeapon : Weapon
	{
		public override float FireRate => 1f;
		public override int BaseDamage => 40;
		public override bool BoneMerge => false;

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		public override void Attack()
		{
			if ( Attacker is DroneEntity drone && Target is ISelectable target )
			{
				drone.Suicide( target );
				LastAttack = 0f;
			}
		}
	}
}
