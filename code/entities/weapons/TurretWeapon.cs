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
		public override string SoundName => "rts.smg.shoot";
		public override float Force => 5f;
		public virtual float RotateSpeed => 20f;

		public Vector3 TargetDirection { get; private set; }
		[Net] public float Recoil { get; private set; }

		public override void Attack()
		{
			Recoil = 1f;

			base.Attack();
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
		public override void ShootEffects( Vector3 position )
		{
			if ( !IsFacingTarget() ) return;
			
			base.ShootEffects( position );
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
				Attacker.SetAnimParameter( "target", TargetDirection );
			}

			if ( Game.IsServer )
			{
				Attacker.SetAnimParameter( "fire", Recoil );
				Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
			}
		}
	}
}
