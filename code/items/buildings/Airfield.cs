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
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/airfield.png" );
		public override int BuildTime => 40;
		public override float MaxHealth => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 600,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/airfield/airfield.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.terryfactory",
			"building.vehiclefactory"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.chinook",
			"upgrade.airfield"
		};
	}
}
