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
		public override int BuildTime => 60;
		public override ResourceType Resource => ResourceType.Wood;
		public override string Model => "models/buildings/headquarters.vmdl";
		public override int Cost => 200;
		public override List<string> Buildables => new()
		{
			"unit.naked"
		};
	}
}
