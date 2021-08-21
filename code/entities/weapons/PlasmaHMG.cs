using Sandbox;

namespace Facepunch.RTS
{
	[Library("weapon_plasma_hmg")]
	public partial class PlasmaHMG : HMG, IBallisticsWeapon
	{
		public override int BaseDamage => 10;
		public override string MuzzleFlash => "particles/weapons/muzzle_flash_plasma/muzzle_flash_plasma.vpcf";
		public override string BulletTracer => "particles/weapons/muzzle_flash_plasma/bullet_trace.vpcf";
		public override DamageFlags DamageType => DamageFlags.Plasma;
		public override string SoundName => "smg.plasma.fire";

		public override void Attack()
		{
			if ( Target is ISelectable target && !target.HasStatus<WeakStatus>() )
			{
				target.ApplyStatus<WeakStatus>( new ModifierData()
				{
					Modifier = 0.2f,
					Duration = 2f
				} );
			}

			base.Attack();
		}
	}
}
