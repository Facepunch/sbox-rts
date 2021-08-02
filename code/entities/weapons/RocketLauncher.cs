using Facepunch.RTS;
using Gamelib.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_rocket_launcher" )]
	public partial class RocketLauncher : Weapon, IBallisticsWeapon
	{
		public override string BulletTracer => null;
		public override float FireRate => 4f;
		public override int BaseDamage => 25;
		public override int HoldType => 5;
		public override string SoundName => "rocketlauncher.fire";
		public override float Force => 20f;

		private Projectile Rocket { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/weapons/rpg/rpg.vmdl" );
		}

		public override void Attack()
		{
			LastAttack = 0f;
			ShootEffects( Target.WorldSpaceBounds.Center );
			LaunchProjectile();
			PlaySound( SoundName );
		}

		protected void LaunchProjectile()
		{
			var muzzle = GetMuzzle();

			if ( muzzle.HasValue )
			{
				Rocket = new Projectile
				{
					BezierCurve = false,
					HitSound = "rocket.explode1",
					Debug = true
				};

				Rocket.Initialize( muzzle.Value.Position, Target, 1f, OnRocketHit );
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

		private void OnRocketHit( Projectile grenade, Entity entity )
		{
			if ( !entity.IsValid() ) return;
			DamageEntity( entity, DamageFlags.Blast, Force, GetDamage() );
		}
	}
}
