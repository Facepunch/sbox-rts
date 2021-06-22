using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_turret_gun")]
	public partial class TurretGun : Weapon
	{
		public override float FireRate => 0.1f;
		public override int BaseDamage => 5;

		public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_smg.shoot" ).SetVolume( 0.5f );
			ShootBullet( 5f, BaseDamage );

			Recoil = 1f;
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		public override void DoImpactEffect( TraceResult trace, float damage )
		{
			// Don't go crazy with impact effects because we fire fast.
			if ( Rand.Float( 1f ) >= 0.5f && Target is IDamageable damageable )
			{
				damageable.DoImpactEffects( trace );
			}
		}


		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();
			
			if ( Attacker.IsValid() )
			{
				Particles.Create( "particles/turret/muzzleflash.vpcf", Attacker, "muzzle" );
			}
		}

		[Event.Tick]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * 20f );
				Attacker.SetAnimVector( "target", TargetDirection );
			}

			Attacker.SetAnimFloat( "fire", Recoil );

			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}
	}
}
