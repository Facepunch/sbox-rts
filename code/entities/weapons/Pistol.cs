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

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 1.5f, BaseDamage );
		}
	}
}
