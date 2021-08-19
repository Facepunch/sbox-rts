using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class ResearchLab : BaseBuilding
	{
		public override string Name => "Research Lab";
		public override string UniqueId => "building.researchlab";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/researchlab.png" );
		public override string Description => "Research new technologies for your empire.";
		public override int BuildTime => 40;
		public override float MaxHealth => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 300,
			[ResourceType.Metal] = 200
		};
		public override string Model => "models/buildings/research_lab/research_lab.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"building.terryfactory"
		};
		public override HashSet<string> Queueables => new()
		{
			"tech.brewing",
			"tech.machinery",
			"tech.extraction",
			"tech.syringes",
			"tech.boring",
			"tech.supplylines",
			"tech.thermalarmor",
			"tech.pyrotechnics",
			"tech.armoredplating",
			"tech.infrastructure",
			"tech.remotedrones",
			"tech.basicballistics",
			"tech.carbines",
			"upgrade.researchlab"
		};
	}
}
