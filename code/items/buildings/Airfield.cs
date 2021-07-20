using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Airfield : BaseBuilding
	{
		public override string Name => "Airfield";
		public override string UniqueId => "building.airfield";
		public override string Description => "Allows you to train various types of aircraft.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/vehiclefactory.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 500,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/vehiclefactory/vehiclefactory.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"building.terryfactory",
			"building.vehiclefactory",
			"tech.airsuperiority"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.chinook"
		};
	}
}
