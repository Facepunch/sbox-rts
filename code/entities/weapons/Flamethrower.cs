using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_flamethrower" )]
	partial class Flamethrower : Weapon
	{
		public override float FireRate => 0.1f;
		public override int BaseDamage => 4;
		public override int HoldType => 2;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			//PlaySound( "rust_pistol.shoot" );
			ShootBullet( 1f, GetDamage() );
		}
	}
}
