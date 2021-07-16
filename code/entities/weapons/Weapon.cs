using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public partial class Weapon : AnimEntity
	{
		[Net] public AnimEntity Attacker { get; set; }
		[Net] public Entity Occupiable { get; set; }
		[Net] public Entity Target { get; set; }
		public virtual bool BoneMerge => true;
		public virtual bool IsMelee => false;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public virtual float RotationTolerance => 0.5f;
		public virtual string MuzzleFlash => "particles/weapons/muzzle_flash/muzzleflash_base.vpcf";
		public virtual string BulletTracer => "particles/weapons/muzzle_flash/bullet_trace.vpcf";
		public virtual float FireRate => 1f;
		public TimeSince LastAttack { get; set; }

		public Weapon()
		{
			EnableAllCollisions = false;
		}

		public virtual bool CanSeeTarget()
		{
			var aimRay = GetAimRay();
			var trace = Trace.Ray( aimRay.Origin, Target.WorldSpaceBounds.Center )
				.EntitiesOnly()
				.HitLayer( CollisionLayer.Debris, false )
				.WithoutTags( "unit" )
				.Ignore( Occupiable )
				.Ignore( Attacker )
				.Run();

			// Did we make it mostly to our target position?
			return (trace.Fraction >= 0.95f);
		}

		public virtual int GetDamage()
		{
			if ( Attacker is UnitEntity unit && unit.Rank != null )
			{
				return BaseDamage + unit.Rank.DamageModifier;
			}

			return BaseDamage;
		}

		public virtual bool CanAttack()
		{
			if ( !CanSeeTarget() ) return false;
			return (LastAttack > FireRate);
		}

		public virtual void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			ShootBullet( 1.5f, GetDamage() );
		}

		public virtual Transform? GetMuzzle()
		{
			if ( Occupiable is IOccupiableEntity occupiable )
			{
				var attachment = occupiable.GetAttackAttachment( Target );
				if ( attachment.HasValue ) return attachment;
			}

			return GetAttachment( "muzzle", true );
		}

		public virtual Ray GetAimRay()
		{
			var attachment = GetMuzzle();

			if ( attachment.HasValue )
			{
				var transform = attachment.Value;

				return new Ray {
					Origin = transform.Position,
					Direction = Target.IsValid() ? (Target.Position - transform.Position).Normal : transform.Rotation.Forward.Normal
				};
			}

			return new Ray {
				Origin = Position,
				Direction = Target.IsValid()? (Target.Position - Position).Normal : Rotation.Forward.Normal
			};
		}

		public virtual void DoImpactEffect( Vector3 position, Vector3 normal, float damage )
		{
			// Don't go crazy with impact effects because we fire fast.
			if ( Rand.Float( 1f ) >= 0.5f && Target is IDamageable damageable )
			{
				damageable.DoImpactEffects( position, normal );
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			if ( IsMelee ) return;

			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				if ( !string.IsNullOrEmpty( MuzzleFlash ) )
				{
					var flash = Particles.Create( MuzzleFlash );
					flash.SetPosition( 0, muzzle.Value.Position );
					flash.SetForward( 0, muzzle.Value.Rotation.Forward );
				}

				if ( !string.IsNullOrEmpty( BulletTracer ) )
				{
					var tracer = Particles.Create( BulletTracer );
					tracer.SetPosition( 0, muzzle.Value.Position );
					tracer.SetPosition( 1, Target.WorldSpaceBounds.Center );
				}
			}
		}

		public virtual void ShootBullet( float force, float damage )
		{
			var aimRay = GetAimRay();
			var endPos = Target.WorldSpaceBounds.Center;
			var damageInfo = DamageInfo.FromBullet( endPos, aimRay.Direction * 100 * force, damage )
				.WithAttacker( Attacker )
				.WithWeapon( this );

			Target.TakeDamage( damageInfo );

			DoImpactEffect( endPos, aimRay.Direction, damage );

			if ( Target is IDamageable damageable && Rand.Float( 1f ) > 0.7f  )
			{
				damageable.CreateDamageDecals( endPos );
			}
		}
	}
}
