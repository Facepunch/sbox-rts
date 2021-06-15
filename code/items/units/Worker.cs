using Sandbox;
using System.Collections.Generic;

namespace RTS.Units
{
	[Library]
	public class Worker : BaseUnit
	{
		public override string Name => "Worker";
		public override string UniqueId => "unit.worker";
		public override bool CanConstruct => true;
		public override string Description => "Gathers Wood, Stone and Beer for your empire and constructs buildings.";
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Stone,
			ResourceType.Metal,
			ResourceType.Beer
		};
		public override HashSet<string> Buildables => new()
		{
			"building.headquarters",
			"building.brewery",
			"building.pub",
			"building.silo",
			"building.terryfactory",
			"building.vehiclefactory"
		};
		public override HashSet<string> Clothing => new()
		{
			"models/citizen_clothes/hat/hat_hardhat.vmdl"
		};
	}
}
