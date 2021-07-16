using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedResearchLab : ResearchLab
	{
		public override string Name => "Advanced Research Lab";
		public override string UniqueId => "building.advancedresearchlab";
		public override Texture Icon => Texture.Load( "textures/rts/icons/researchlab.png" );
		public override string Description => "Research extraordinary new technologies for your empire.";
		public override string Model => "models/buildings/research_lab/research_lab.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"tech.pyrotechnics",
			"tech.cryogenics",
			"tech.overvoltage",
			"tech.darkenergy"
		};
	}
}
