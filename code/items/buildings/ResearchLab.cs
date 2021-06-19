using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class ResearchLab : BaseBuilding
	{
		public override string Name => "Research Lab";
		public override string UniqueId => "building.researchlab";
		public override Texture Icon => Texture.Load( "textures/rts/icons/researchlab.png" );
		public override string Description => "Research extraordinary new technologies for your empire.";
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 400,
			[ResourceType.Metal] = 400,
			[ResourceType.Plasma] = 300
		};
		public override string Model => "models/buildings/research_lab/research_lab.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters"
		};
	}
}
