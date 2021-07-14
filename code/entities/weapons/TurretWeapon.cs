using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_turret")]
	public partial class TurretWeapon : Weapon
	{
		public override float FireRate => 0.1f;
		public override int BaseDamage => 5;
		public override bool BoneMerge => false;

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
			if ( Occupiable.IsValid() )
			{
				return base.GetMuzzle();
			}

			return Attacker.GetAttachment( "muzzle", true );
		}

		[Event.Tick]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * 20f );
				Attacker.SetAnimVector( "target", TargetDirection );
			}

			Attacker.SetAnimFloat( "fire", Recoil );

			Recoil = Recoil.LerpTo( 0f, Time.Delta * 2f );
		}
	}
}
