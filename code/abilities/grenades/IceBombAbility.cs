using Sandbox;

namespace Facepunch.RTS
{
	[Library( "ability_ice_bomb" )]
	public class IceBombAbility : BaseGrenadeAbility
	{
		public override string Name => "Ice Bomb";
		public override string Description => "A grenade that deals area of effect freeze damage and slows enemies.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 10f;
		public override float MaxDistance => 750f;
		public override float Duration => 1f;
		public override float AreaOfEffectRadius => 300f;
		public override string ExplosionEffect => "particles/weapons/explosion_ground_ice/explosion_ground_ice.vpcf";
		public override string AreaEffect => null;

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				Statuses.Apply<FreezingStatus>( TargetInfo.Origin, AreaOfEffectRadius, new FreezingData()
				{
					SpeedReduction = 100f,
					Interval = 0.2f,
					Damage = 2f,
					Duration = 5f
				} );
			}

			base.OnFinished();
		}
	}
}
