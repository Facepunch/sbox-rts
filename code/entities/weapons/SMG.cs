﻿using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library("weapon_smg")]
	partial class SMG : Weapon
	{
		public override float FireRate => 0.25f;
		public override int BaseDamage => 3;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_smg.shoot" );
			ShootBullet( 1.5f, GetDamage() );
		}
	}
}
