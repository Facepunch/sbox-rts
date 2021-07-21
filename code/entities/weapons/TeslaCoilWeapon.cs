using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	[Library("weapon_tesla_coil")]
	public partial class TeslaCoilWeapon : Weapon
	{
		public override float FireRate => 2f;
		public override int BaseDamage => 40;
		public override bool BoneMerge => false;

		public override void Attack()
		{
			LastAttack = 0f;

			//PlaySound( "rust_smg.shoot" ).SetVolume( 0.5f );

			DamageInRange();
		}

		public override Transform? GetMuzzle()
		{
			return Attacker.Transform;
		}

		private async void DamageInRange()
		{
			if ( Attacker is not BuildingEntity building ) return;

			var origin = Attacker.WorldSpaceBounds.Center;

			var pulse = Particles.Create( "particles/tesla_coil/tesla_ring.vpcf" );
			pulse.SetPosition( 0, origin );

			var targets = Physics.GetEntitiesInSphere( Attacker.Position, building.Item.AttackRadius )
				.OfType<ISelectable>()
				.Where( v => building.IsEnemy( v ) )
				.ToList();

			var targetCount = targets.Count;
			var damage = GetDamage() / targetCount;

			for ( int i = 0; i < targetCount; i++ )
			{
				var target = targets[i];

				var bolt = Particles.Create( "particles/weapons/electric_bolt/electric_bolt.vpcf" );
				bolt.SetPosition( 0, origin );
				bolt.SetPosition( 1, target.WorldSpaceBounds.Center );

				DamageEntity( (Entity)target, DamageFlags.Shock, 5f, damage );

				await GameTask.Delay( Rand.Int( 0, 5 ) );
			}
		}
	}
}
