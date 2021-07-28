using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_sniper" )]
	partial class Sniper : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 3f;
		public override int BaseDamage => 30;
		public override int HoldType => 2;
		public override string SoundName => "rust_smg.shoot";
		public override float Force => 5f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}
	}
}
