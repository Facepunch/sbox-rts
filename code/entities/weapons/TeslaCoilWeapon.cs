using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_tesla_coil")]
	public partial class TeslaCoilWeapon : Weapon
	{
		public override float FireRate => 2f;
		public override int BaseDamage => 20;
		public override bool BoneMerge => false;

		public override void Attack()
		{
			LastAttack = 0f;

			//PlaySound( "rust_smg.shoot" ).SetVolume( 0.5f );
			DamageTarget( DamageFlags.Shock, 5f, GetDamage() );
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.Transform;
		}

		[ClientRpc]
		public override void ShootEffects()
		{
			
		}
	}
}
