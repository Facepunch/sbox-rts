using Sandbox;
using System;

namespace RTS
{
	[Library("weapon_pistol")]
	partial class Pistol : Weapon
	{
		public override float FireRate => 1.0f;
		public override int BaseDamage => 8;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void Attack( Entity target )
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( target, 0.05f, 1.5f, BaseDamage, 3.0f );
		}
	}
}
