using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class VehicleFactory : BaseBuilding
	{
		public override string Name => "Vehicle Factory";
		public override string UniqueId => "building.vehiclefactory";
		public override string Description => "Allows you to train various types of vehicle.";
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 500,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/vehiclefactory_future/vehiclefactory.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"building.terryfactory",
			"tech.wheels"
		};
		public override HashSet<string> Buildables => new()
		{
			"unit.scout"
		};
	}
}
