using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class ResearchLab : BaseBuilding
	{
		public override string Name => "Research Lab";
		public override string UniqueId => "building.researchlab";
		public override Texture Icon => Texture.Load( "textures/rts/icons/researchlab.png" );
		public override string Description => "Research new technologies for your empire.";
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 400,
			[ResourceType.Metal] = 600
		};
		public override string Model => "models/buildings/research_lab/research_lab.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"building.terryfactory",
			"tech.machinery"
		};
		public override HashSet<string> Queueables => new()
		{
			"tech.brewing",
			"tech.extraction",
			"tech.syringes",
			"tech.thermalarmor",
			"tech.pyrotechnics",
			"tech.armoredplating",
			"tech.infrastructure",
			"tech.basicballistics",
			"tech.carbines",
			"upgrade.researchlab"
		};
	}
}
