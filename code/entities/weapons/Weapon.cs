using Sandbox;
using System;
using System.Collections.Generic;

namespace RTS
{
	public partial class Weapon : AnimEntity
	{
		[Net] public UnitEntity Unit { get; set; }
		public virtual bool IsMelee => false;
		public virtual int BaseDamage => 10;
		public virtual int HoldType => 1;
		public virtual float FireRate => 1f;
		public TimeSince LastAttack { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public virtual bool CanAttack()
		{
			return (LastAttack > FireRate);
		}

		public virtual void Attack( Entity target )
		{
			LastAttack = 0f;

			ShootEffects();
			ShootBullet( target, 0.05f, 1.5f, BaseDamage, 3.0f );
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

		public virtual void ShootBullet( Entity target, float spread, float force, float damage, float bulletSize )
		{
			var forward = Unit.Rotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach ( var tr in TraceBullet( Position, target.WorldSpaceBounds.Center, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( tr.Entity == target )
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
						.UsingTraceResult( tr )
						.WithAttacker( Unit )
						.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
		{
			bool InWater = Physics.TestPointContents( start, CollisionLayer.Water );

			var tr = Trace.Ray( start, end )
				.UseHitboxes()
				.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Unit )
				.Ignore( this )
				.Size( radius )
				.Run();

			yield return tr;
		}
	}
}
