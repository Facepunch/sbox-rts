using Sandbox;

namespace Facepunch.RTS
{
	public class BaseGrenadeAbility : BaseAbility
	{
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override AbilityTargetTeam TargetTeam => AbilityTargetTeam.Enemy;
		public virtual string ExplosionEffect => "particles/weapons/explosion_ground_small/explosion_ground_small.vpcf";
		public virtual string StatusEffect => null;
		public virtual string AreaEffect => null;

		private Grenade Grenade { get; set; }

		public override void OnStarted()
		{
			if ( Host.IsServer && User is UnitEntity unit )
			{
				unit.Weapon.DummyAttack();

				Grenade = new Grenade
				{
					ExplosionEffect = ExplosionEffect
				};

				var muzzle = unit.Weapon.GetMuzzle();

				Grenade.Initialize( muzzle.Value.Position, TargetInfo.Origin, Duration );
			}

			base.OnStarted();
		}

		public override void OnCancelled()
		{
			if ( Grenade.IsValid() )
			{
				Grenade.Delete();
				Grenade = null;
			}

			base.OnCancelled();
		}

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				if ( !string.IsNullOrEmpty( StatusEffect ) )
					Statuses.Apply( StatusEffect, TargetInfo.Origin, AreaOfEffectRadius );

				if ( !string.IsNullOrEmpty( AreaEffect ) )
					CreateAreaEffect( TargetInfo.Origin, 1f );
			}

			base.OnFinished();
		}

		private async void CreateAreaEffect( Vector3 position, float duration )
		{
			var particles = Particles.Create( AreaEffect );
			particles.SetPosition( 0, TargetInfo.Origin );
			particles.SetPosition( 1, new Vector3( AreaOfEffectRadius, 0f, 0f ) );

			await GameTask.DelaySeconds( duration );

			particles.Destroy();
		}
	}
}
