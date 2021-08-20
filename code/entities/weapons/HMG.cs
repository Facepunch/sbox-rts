using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library("weapon_hmg")]
	partial class HMG : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 0.3f;
		public override int BaseDamage => 8;
		public override int HoldType => 3;
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 4f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/machinegun/machinegun.vmdl" );
		}
	}
}
