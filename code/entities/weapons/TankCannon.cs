using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_tank_cannon")]
	public partial class TankCannon : Weapon
	{
		public override float FireRate => 3f;
		public override int BaseDamage => 30;
		public override bool BoneMerge => false;
		public override float RotationTolerance => 360f;

		public Vector3 TargetDirection { get; private set; }

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_smg.shoot" ).SetVolume( 0.5f );
			ShootBullet( 5f, GetDamage() );
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

		public override bool CanAttack()
		{
			var goalDirection = (Target.Position - Attacker.Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > 2f )
				return false;

			return base.CanAttack();
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

		[Event.Tick.Server]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * 10f );
				Attacker.SetAnimVector( "target", Attacker.Transform.NormalToLocal( TargetDirection ) );
			}
		}
	}
}
