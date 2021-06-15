using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class Headquarters : BaseBuilding
	{
		public override string Name => "Headquarters";
		public override string UniqueId => "building.headquarters";
		public override string Description => "This is the heart of your empire. Protect it at all costs.";
		public override bool CanDepositResources => true;
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 1000,
			[ResourceType.Metal] = 500
		};
		public override string Model => "models/buildings/headquarters_future/headquarters.vmdl";
		public override HashSet<string> Buildables => new()
		{
			"unit.worker",
			"unit.scientist",
			"tech.brewing",
			"tech.clothing",
			"tech.wheels"
		};
	}
}
