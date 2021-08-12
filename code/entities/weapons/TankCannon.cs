using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_tank_cannon")]
	public partial class TankCannon : Weapon, IBallisticsWeapon
	{
		public override float FireRate => 3f;
		public override int BaseDamage => 30;
		public override bool BoneMerge => false;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash/muzzle_large/muzzleflash_large.vpcf";
		public override string BulletTracer => null;
		public override float RotationTolerance => 360f;
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 5f;
		public virtual float RotateSpeed => 10f;

		public Vector3 TargetDirection { get; private set; }

		public override Transform? GetMuzzle()
		{
			if ( Occupiable.IsValid() )
			{
				return base.GetMuzzle();
			}

			return Attacker.GetAttachment( "muzzle", true );
		}

		[ClientRpc]
		public override void ShootEffects( Vector3 position )
		{
			var explosion = Particles.Create( "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf" );
			explosion.SetPosition( 0, position );
			base.ShootEffects( position );
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
