using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class Pub : BaseBuilding
	{
		public override string Name => "Pub";
		public override string UniqueId => "building.pub";
		public override string Description => "Increases the maximum population of your empire.";
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 150
		};
		public override string Model => "models/buildings/pub_future/pub.vmdl";
		public override List<string> Dependencies => new()
		{
			"construct.headquarters",
			"tech.brewing"
		};
	}
}
