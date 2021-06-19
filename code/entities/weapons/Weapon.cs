using Sandbox;
using System;
using System.Collections.Generic;

namespace RTS
{
	public partial class Weapon : AnimEntity
	{
		[Net] public AnimEntity Attacker { get; set; }
		[Net] public Entity Target { get; set; }
		public virtual bool IsMelee => false;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public virtual float FireRate => 1f;
		public TimeSince LastAttack { get; set; }

		public virtual bool CanSeeTarget()
		{
			var aimRay = GetAimRay();
			var trace = Trace.Ray( aimRay.Origin, Target.WorldSpaceBounds.Center )
				.EntitiesOnly()
				.HitLayer( CollisionLayer.Debris, false )
				.Ignore( Attacker )
				.Ignore( this )
				.Run();

			return (trace.Entity == Target);
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
			ShootBullet( 1.5f, BaseDamage );
		}

		public virtual Transform? GetMuzzle()
		{
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

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			if (!IsMelee)
			{
				Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
			}
		}

		public virtual void ShootBullet( float force, float damage )
		{
			var aimRay = GetAimRay();
			var trace = Trace.Ray( aimRay.Origin, Target.WorldSpaceBounds.Center )
				.EntitiesOnly()
				.HitLayer( CollisionLayer.Debris, false )
				.Ignore( Attacker )
				.Ignore( this )
				.Run();

			Log.Info( trace.Entity.ToString() );

			trace.Surface.DoBulletImpact( trace );

			if ( trace.Entity == Target )
			{
				var damageInfo = DamageInfo.FromBullet( trace.EndPos, trace.Direction * 100 * force, damage )
					.UsingTraceResult( trace )
					.WithAttacker( Attacker )
					.WithWeapon( this );

				trace.Entity.TakeDamage( damageInfo );
			}
		}
	}
}
