using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class DroneHub : BaseBuilding
	{
		public override string Name => "Drone Hub";
		public override string UniqueId => "building.dronehub";
		public override string Description => "Allows you to train various types of drones.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/vehiclefactory.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 400,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/drone_hub/drone_hub.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"building.terryfactory",
			"tech.remotedrones"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.minerdrone",
			"unit.bubbledrone"
		};
	}
}
