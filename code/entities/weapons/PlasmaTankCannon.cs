using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_plasma_tank_cannon")]
	public partial class PlasmaTankCannon : TankCannon, IBallisticsWeapon
	{
		public override float FireRate => 3.5f;
		public override int BaseDamage => 35;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash_plasma/muzzle_large/muzzleflash_large.vpcf";
		public override string BulletTracer => "particles/weapons/muzzle_flash_plasma/bullet_trace.vpcf";
		public override DamageFlags DamageType => DamageFlags.Blast | DamageFlags.Plasma;

		protected override void OnRocketHit( Projectile grenade, Entity entity )
		{
			base.OnRocketHit( grenade, entity );

			CreateAreaEffect( grenade.Position, 3f );
		}

		private async void CreateAreaEffect( Vector3 position, float duration )
		{
			var particles = Particles.Create( "particles/weapons/plasma_ground/plasma_ground.vpcf" );
			particles.SetPosition( 0, position );
			particles.SetPosition( 1, new Vector3( 250f, 0f, 0f ) );

			Statuses.Apply<WeakStatus>( position, 250f, new ModifierData()
			{
				Modifier = 0.2f,
				Duration = duration
			} );

			await GameTask.DelaySeconds( duration );

			particles.Destroy();
		}
	}
}
