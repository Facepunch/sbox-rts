using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class GraphiteArmor : HealthIncreaseTech<IDroneUnit>
	{
		public override string Name => "Graphite Armor";
		public override string UniqueId => "tech.graphitearmor";
		public override string Description => "Grants an additional 20 HP to all drone units.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override float Health => 20f;
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
	}
}
