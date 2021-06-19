using Sandbox;
using System;

namespace RTS
{
	[Library("weapon_turret_gun")]
	public partial class TurretGun : Weapon
	{
		public override float FireRate => 3f;
		public override int BaseDamage => 12;

		public Vector3 TargetDirection { get; private set; }

		public override void Attack()
		{
			LastAttack = 0f;

			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 1.5f, BaseDamage );
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.GetAttachment( "muzzle", true );
		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();
			
			if ( Attacker.IsValid() )
			{
				Particles.Create( "particles/pistol_muzzleflash.vpcf", Attacker, "muzzle" );
			}
		}

		[Event.Tick]
		private void UpdateTargetRotation()
		{
			if ( Target.IsValid() )
			{
				TargetDirection = TargetDirection.LerpTo( (Target.Position - Attacker.Position).Normal, Time.Delta * 20f );
				Attacker.SetAnimVector( "target", TargetDirection );
			}
		}
	}
}
