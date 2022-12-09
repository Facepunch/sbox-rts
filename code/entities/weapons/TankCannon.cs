using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_tank_cannon")]
	public partial class TankCannon : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 2f;
		public override int BaseDamage => 30;
		public override bool BoneMerge => false;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash/muzzle_large/muzzleflash_large.vpcf";
		public override string BulletTracer => null;
		public override float RotationTolerance => 360f;
		public override string SoundName => "rocketlauncher.fire";
		public override string DamageType => "blast";
		public override float Force => 5f;
		public virtual float RotateSpeed => 10f;

		public Vector3 TargetDirection { get; private set; }

		protected Projectile Rocket { get; set; }

		public override Transform? GetMuzzle()
		{
			if ( Occupiable.IsValid() )
			{
				return base.GetMuzzle();
			}

			return Attacker.GetAttachment( "muzzle", true );
		}

		public override void Attack()
		{
			base.Attack();

			LaunchProjectile();
		}

		public override bool CanAttack()
		{
			var goalDirection = (Target.Position - Attacker.Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > ( 1f / RotateSpeed) )
				return false;

			return base.CanAttack();
		}

		protected void LaunchProjectile()
		{
			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				Rocket = new Projectile
				{
					BezierCurve = false,
					FaceDirection = true,
					HitSound = "rocket.explode1"
				};

				Rocket.SetModel( "models/weapons/rpg_rocket/rpg_rocket.vmdl" );
				Rocket.Initialize( muzzle.Value.Position, Target, 0.4f, OnRocketHit );
			}
		}

		protected override void OnDestroy()
		{
			if ( Rocket.IsValid() )
			{
				Rocket.Delete();
				Rocket = null;
			}

			base.OnDestroy();
		}

		protected virtual void OnRocketHit( Projectile grenade, Entity entity )
		{
			if ( !entity.IsValid() ) return;
			DamageEntity( entity, DamageType, Force, GetDamage() );
		}

		[Event.Tick.Server]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * RotateSpeed );
				Attacker.SetAnimParameter( "target", Attacker.Transform.NormalToLocal( TargetDirection ) );
			}
		}
	}
}
