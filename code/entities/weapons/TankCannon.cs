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
		public override string MuzzleFlash => "particles/weapons/muzzle_flash/muzzle_large/muzzleflash_large.vpcf";
		public override string BulletTracer => null;
		public override float RotationTolerance => 360f;
		public virtual float RotateSpeed => 10f;

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
			if ( Occupiable.IsValid() )
			{
				return base.GetMuzzle();
			}

			return Attacker.GetAttachment( "muzzle", true );
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			var explosion = Particles.Create( "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf" );
			explosion.SetPosition( 0, Target.Position );

			base.ShootEffects();
		}

		public override void DoImpactEffect( Vector3 position, Vector3 normal, float damage )
		{
			// Don't go crazy with impact effects because we fire fast.
			if ( Rand.Float( 1f ) >= 0.5f && Target is IDamageable damageable )
			{
				damageable.DoImpactEffects( position, normal );
			}
		}

		public override bool CanAttack()
		{
			var goalDirection = (Target.Position - Attacker.Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > ( 1f / RotateSpeed) )
				return false;

			return base.CanAttack();
		}

		[Event.Tick.Server]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * RotateSpeed );
				Attacker.SetAnimVector( "target", Attacker.Transform.NormalToLocal( TargetDirection ) );
			}
		}
	}
}
