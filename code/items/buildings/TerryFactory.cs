using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class TerryFactory : BaseBuilding
	{
		public override string Name => "Terry Factory";
		public override string UniqueId => "building.terryfactory";
		public override string Description => "Allows you to train various basic Terrys.";
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200
		};
		public override string Model => "models/buildings/terryfactory_future/terryfactory.vmdl";
		public override List<string> Dependencies => new()
		{
			"building.headquarters"
		};
		public override List<string> Buildables => new()
		{
			"unit.naked"
		};
	}
}
