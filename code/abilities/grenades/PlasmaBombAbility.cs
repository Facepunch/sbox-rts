using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_plasma_bomb" )]
	public class PlasmaBombAbility : BaseGrenadeAbility
	{
		public override string Name => "Plasma Bomb";
		public override string Description => "A grenade that deals area of effect plasma damage.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/heal.png" );
		public override float Cooldown => 60f;
		public override float MaxDistance => 750f;
		public override float Duration => 1f;
		public override float AreaOfEffectRadius => 300f;
		public override string ExplosionEffect => null;
		public override string AreaEffect => "particles/weapons/plasma_ground/plasma_ground.vpcf";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 20
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.darkenergy"
		};

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				Statuses.Apply<WeakStatus>( TargetInfo.Origin, AreaOfEffectRadius, new ModifierData()
				{
					Modifier = 0.2f,
					Duration = 5f
				} );
			}

			base.OnFinished();
		}
	}
}
