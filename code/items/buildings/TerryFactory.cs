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
		public override ResourceType Resource => ResourceType.Wood;
		public override int Cost => 200;
		public override List<string> Dependencies => new()
		{
			"construct.headquarters"
		};
		public override List<string> Buildables => new()
		{
			"unit.naked"
		};
	}
}
