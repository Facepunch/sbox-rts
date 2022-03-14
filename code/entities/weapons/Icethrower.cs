using Sandbox;

namespace Facepunch.RTS
{
	[Library( "weapon_icethrower" )]
	partial class Icethrower : Weapon
	{
		public override float FireRate => 0.1f;
		public override int BaseDamage => 3;
		public override int HoldType => 2;
		public override string SoundName => null;
		public override float Force => 2f;

		private Particles Ice { get; set; }
		private RealTimeUntil KillIceTime { get; set; }
		private RealTimeUntil NextFreezeTime { get; set; }
		private Sound SoundLoop { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void Attack()
		{
			if ( NextFreezeTime )
			{
				Statuses.Apply<FreezingStatus>( Target.Position, 128f, new FreezingData()
				{
					SpeedReduction = 100,
					Interval = 0.3f,
					Duration = 3f,
					Damage = 0.1f
				} );

				NextFreezeTime = 3f;
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
				if ( Ice == null )
				{
					Ice = Particles.Create( "particles/weapons/ice_blast/ice_blast.vpcf" );
					Ice.SetPosition( 0, muzzle.Value.Position );
					SoundLoop = PlaySound( "flamethrower.loop" );
				}

				Ice.SetPosition( 1, position );

				KillIceTime = GetFireRate() * 2f;
			}
		}

		protected override void OnDestroy()
		{
			RemoveParticles();

			base.OnDestroy();
		}

		private void RemoveParticles()
		{
			SoundLoop.Stop();
			Ice?.Destroy();
			Ice = null;
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( Ice == null ) return;

			if ( !Target.IsValid() )
			{
				RemoveParticles();
				return;
			}

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				Ice.SetPosition( 0, muzzle.Value.Position );
			}

			if ( KillIceTime )
			{
				RemoveParticles();
			}
		}
	}
}
