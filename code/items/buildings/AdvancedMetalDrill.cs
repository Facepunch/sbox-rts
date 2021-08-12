using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedMetalDrill : BaseBuilding
	{
		public override string Name => "Advanced Metal Drill";
		public override string UniqueId => "building.advancedmetaldrill";
		public override string Model => "models/buildings/brewery/brewery.vmdl";
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Metal, 2 }},
			Interval = 5f
		};
	}
}
