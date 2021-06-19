using Sandbox;
using System;
using System.Collections.Generic;

namespace RTS
{
	public partial class Weapon : AnimEntity
	{
		[Net] public ModelEntity Attacker { get; set; }
		[Net] public Entity Target { get; set; }
		public virtual bool IsMelee => false;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public virtual float FireRate => 1f;
		public TimeSince LastAttack { get; set; }

		public virtual bool CanAttack()
		{
			return (LastAttack > FireRate);
		}

		public virtual void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			ShootBullet( 1.5f, BaseDamage, 3.0f );
		}

		public virtual Transform? GetMuzzle()
		{
			return GetAttachment( "muzzle", true );
		}

		public virtual Ray GetOriginAndDirection()
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

		public virtual void ShootBullet( float force, float damage, float bulletSize )
		{
			var ray = GetOriginAndDirection();

			foreach ( var tr in TraceBullet( ray.Origin, Target.WorldSpaceBounds.Center, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( tr.Entity == Target )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, tr.Direction * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Attacker )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool isInWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !isInWater )
				.Ignore( Attacker )
				.Ignore( this )
				.Size( radius )
				.Run();

			yield return tr;
		}
	}
}
