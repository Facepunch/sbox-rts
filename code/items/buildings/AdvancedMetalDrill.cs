using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedMetalDrill : MetalDrill
	{
		public override string Name => "Advanced Metal Drill";
		public override string UniqueId => "building.advancedmetaldrill";
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Metal, 2 }},
			FinishSound = "rts.generator.collect1",
			LoopSound = "rts.generator.drillingloop",
			Interval = 5f
		};
	}
}
