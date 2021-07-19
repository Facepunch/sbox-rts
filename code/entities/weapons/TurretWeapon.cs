using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_turret")]
	public partial class TurretWeapon : Weapon
	{
		public override float FireRate => 0.3f;
		public override int BaseDamage => 8;
		public override bool BoneMerge => false;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash/muzzle_large/muzzleflash_large.vpcf";
		public virtual float RotateSpeed => 20f;

		public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_smg.shoot" ).SetVolume( 0.5f );
			ShootBullet( 5f, GetDamage() );

			Recoil = 1f;
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		public override bool CanAttack()
		{
			return IsFacingTarget() && base.CanAttack();
		}

		[ClientRpc]
		public override void ShootEffects()
		{
			if ( !IsFacingTarget() ) return;
			
			base.ShootEffects();
		}

		private bool IsFacingTarget()
		{
			var goalDirection = (Target.Position - Attacker.Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > ( 1f / RotateSpeed) )
				return false;

			return true;
		}
		
		[Event.Tick]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * RotateSpeed );
				Attacker.SetAnimVector( "target", TargetDirection );
			}

			if ( IsServer )
			{
				Attacker.SetAnimFloat( "fire", Recoil );
				Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
			}
		}
	}
}
