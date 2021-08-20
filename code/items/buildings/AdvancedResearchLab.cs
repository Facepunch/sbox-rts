using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedResearchLab : ResearchLab
	{
		public override string Name => "Advanced Research Lab";
		public override string UniqueId => "building.advancedresearchlab";
		public override string Description => "Research extraordinary new technologies for your empire.";
		public override string Model => "models/buildings/research_lab/research_lab_level2.vmdl";
		public override string[] ActsAsProxyFor => new string[] { "building.researchlab" };
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"tech.advancedballistics",
			"tech.airsuperiority",
			"tech.advancedboring",
			"tech.armageddon",
			"tech.cryogenics",
			"tech.overvoltage",
			"tech.darkenergy"
		};
	}
}
