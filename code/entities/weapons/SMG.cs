using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library("weapon_smg")]
	partial class SMG : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 0.15f;
		public override int BaseDamage => 2;
		public override int HoldType => 2;
		public override string SoundName => "rust_smg.shoot";
		public override float Force => 2f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/m4/m4.vmdl" );
		}
	}
}
