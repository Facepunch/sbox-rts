using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedFusionReactor : FusionReactor
	{
		public override string Name => "Advanced Fusion Reactor";
		public override string UniqueId => "building.advancedfusionreactor";
		public override string Model => "models/buildings/fusion_reactor/fusion_reactor_level2.vmdl";
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Plasma, 1 }},
			FinishSound = "rts.generator.collect1",
			LoopSound = "rts.generator.drillingloop",
			Interval = 10f
		};
	}
}
