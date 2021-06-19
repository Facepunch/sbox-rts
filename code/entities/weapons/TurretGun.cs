using Sandbox;
using System;

namespace RTS
{
	[Library("weapon_turret_gun")]
	public partial class TurretGun : Weapon
	{
		public Rotation TargetRotation { get; private set; }
		public override float FireRate => 3f;
		public override int BaseDamage => 12;

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 1.5f, BaseDamage, 3.0f );
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		public override bool CanAttack()
		{
			var targetDirection = Target.Position - Attacker.Position;
			var idealRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );

			if ( !idealRotation.Distance( TargetRotation ).AlmostEqual( 0f, 1f ) )
				return false;

			return base.CanAttack();
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();
			
			if ( Attacker.IsValid() )
			{
				//Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );
			}
		}

		[Event.Frame]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				var targetDirection = Target.Position - Attacker.Position;
				TargetRotation = Rotation.LookAt( targetDirection.Normal, Vector3.Up );
			}

			var gunBone = Attacker.GetBoneTransform( "gun", true );
			gunBone.Rotation = Rotation.Lerp( gunBone.Rotation, TargetRotation, Time.Delta * 20f );
			Attacker.SetBoneTransform( "gun", gunBone, true );
		}
	}
}
