using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_sniper" )]
	partial class Sniper : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 3f;
		public override int BaseDamage => 30;
		public override int HoldType => 4;
		public override string SoundName => "rust_smg.shoot";
		public override float Force => 5f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/sniper/sniper.vmdl" );
		}
	}
}
