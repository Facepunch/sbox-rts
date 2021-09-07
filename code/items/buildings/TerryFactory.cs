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
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/terryfactory.png" );
		public override int BuildTime => 30;
		public override float MaxHealth => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150
		};
		public override string Model => "models/buildings/terryfactory/terryfactory.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre"
		};
		public override HashSet<string> Queueables => new()
		{
			"unit.grunt",
			"unit.assault",
			"unit.medic",
			"tech.syringes",
			"tech.kevlararmor",
			"upgrade.terryfactory"
		};
	}
}
