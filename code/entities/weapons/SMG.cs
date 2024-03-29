﻿using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library("weapon_smg")]
	public partial class SMG : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 0.3f;
		public override int BaseDamage => 4;
		public override int HoldType => 2;
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 2f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/m4/m4.vmdl" );
		}
	}
}
