using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class TerryFactory : BaseBuilding
	{
		public override string Name => "Terry Factory";
		public override string UniqueId => "building.terryfactory";
		public override string Description => "Allows you to train various basic Terrys.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/terryfactory.png" );
		public override int BuildTime => 5;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200
		};
		public override string Model => "models/buildings/terryfactory/terryfactory.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.cannonfodder",
			"unit.assault",
			"unit.medic",
			"unit.grenadier",
			"unit.pyromaniac"
		};
	}
}
