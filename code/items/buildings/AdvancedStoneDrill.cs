using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedStoneDrill : StoneDrill
	{
		public override string Name => "Advanced Stone Drill";
		public override string UniqueId => "building.advancedstonedrill";
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Stone, 4 }},
			FinishSound = "rts.generator.collect1",
			LoopSound = "rts.generator.drillingloop",
			Interval = 5f
		};
	}
}
