using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class GraphiteArmor : HealthIncreaseTech
	{
		public override string Name => "Graphite Armor";
		public override string UniqueId => "tech.graphitearmor";
		public override string Description => "Grants an additional 20 HP to all drone units.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override float Health => 20f;
		public override string Tag => "drone";
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 100
		};
	}
}
