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

		private Particles Fire { get; set; }
		private RealTimeUntil KillFireTime { get; set; }

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

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				if ( Fire == null )
				{
					Fire = Particles.Create( "particles/weapons/flamethrower.vpcf" );
					Fire.SetPosition( 0, muzzle.Value.Position );
				}

				Fire.SetPosition( 1, Target.Position );

				KillFireTime = 0.5f;
			}
		}

		protected override void OnDestroy()
		{
			Fire?.Destroy();
			Fire = null;

			base.OnDestroy();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( KillFireTime && Fire != null )
			{
				Fire.Destroy();
				Fire = null;
			}
		}
	}
}
