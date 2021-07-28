using Facepunch.RTS;
using Gamelib.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "weapon_grenade_launcher" )]
	public partial class GrenadeLauncher : Weapon, IBallisticsWeapon
	{
		public override string BulletTracer => null;
		public override float FireRate => 2f;
		public override int BaseDamage => 15;
		public override int HoldType => 2;
		public override string SoundName => "rust_pistol.shoot";
		public override float Force => 10f;

		private Projectile Grenade { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
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
				Grenade = new Projectile();
				Grenade.Initialize( muzzle.Value.Position, Target, 1f, OnGrenadeHit );
			}
		}

		protected override void OnDestroy()
		{
			if ( Grenade.IsValid() )
			{
				Grenade.Delete();
				Grenade = null;
			}

			base.OnDestroy();
		}

		private void OnGrenadeHit( Projectile grenade, Entity entity )
		{
			if ( !entity.IsValid() ) return;
			DamageEntity( entity, DamageFlags.Blast, Force, GetDamage() );
		}
	}
}
