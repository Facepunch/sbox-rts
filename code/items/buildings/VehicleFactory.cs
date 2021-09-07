using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class VehicleFactory : BaseBuilding
	{
		public override string Name => "Vehicle Factory";
		public override string UniqueId => "building.vehiclefactory";
		public override string Description => "Allows you to train various types of vehicle.";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/vehiclefactory.png" );
		public override float MaxHealth => 800f;
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 300,
			[ResourceType.Metal] = 150,
		};
		public override string Model => "models/buildings/vehiclefactory/vehiclefactory.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.terryfactory",
			"tech.machinery"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.ranger",
			"unit.buggy",
			"tech.armoredplating",
			"upgrade.vehiclefactory"
		};
	}
}
