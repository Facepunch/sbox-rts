using Facepunch.RTS;
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
		public override string SoundName => null;
		public override float Force => 2f;

		private Particles Fire { get; set; }
		private RealTimeUntil KillFireTime { get; set; }
		private RealTimeUntil NextBurnTime { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void Attack()
		{
			if ( NextBurnTime )
			{
				Statuses.Apply<BurningStatus>( Target.Position, 128f, new BurningData()
				{
					Interval = 0.3f,
					Duration = 3f,
					Damage = 1f
				} );

				NextBurnTime = 3f;
			}

			base.Attack();
		}

		[ClientRpc]
		public override void ShootEffects( Vector3 position )
		{
			Host.AssertClient();

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				if ( Fire == null )
				{
					Fire = Particles.Create( "particles/weapons/flamethrower/flamethrower.vpcf" );
					Fire.SetPosition( 0, muzzle.Value.Position );
				}

				Fire.SetPosition( 1, position );

				KillFireTime = FireRate * 2f;
			}
		}

		protected override void OnDestroy()
		{
			Fire?.Destroy();
			Fire = null;

			base.OnDestroy();
		}

		private void RemoveParticles()
		{
			Fire.Destroy();
			Fire = null;
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Fire == null ) return;

			if ( !Target.IsValid() )
			{
				RemoveParticles();
				return;
			}

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				Fire.SetPosition( 0, muzzle.Value.Position );
			}

			if ( KillFireTime  )
			{
				RemoveParticles();
			}
		}
	}
}
