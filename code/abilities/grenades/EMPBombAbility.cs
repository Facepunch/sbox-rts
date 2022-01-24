using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_emp_bomb" )]
	public class EMPBombAbility : BaseGrenadeAbility
	{
		public override string Name => "EMP Bomb";
		public override string Description => "A grenade that deals area of effect electric damage and shocks enemies.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/heal.png" );
		public override float Cooldown => 10f;
		public override float MaxDistance => 750f;
		public override float Duration => 1f;
		public override float AreaOfEffectRadius => 300f;
		public override string ExplosionEffect => "particles/weapons/explosion_ground_electric/explosion_ground_electric.vpcf";
		public override string AreaEffect => "particles/weapons/ice_ground/ice_ground_base.vpcf";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 10
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.overvoltage"
		};

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				Statuses.Apply<ShockStatus>( TargetInfo.Origin, AreaOfEffectRadius, new ModifierData()
				{
					Modifier = 2f,
					Duration = 5f
				} );
			}

			base.OnFinished();
		}
	}
}
