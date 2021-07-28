using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library("weapon_smg")]
	partial class SMG : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 0.3f;
		public override int BaseDamage => 5;
		public override int HoldType => 2;
		public override string SoundName => "rust_smg.shoot";
		public override float Force => 2f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}
	}
}
