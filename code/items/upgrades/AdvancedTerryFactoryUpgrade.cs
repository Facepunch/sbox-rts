using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class AdvancedTerryFactoryUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Terry Factory";
		public override string UniqueId => "upgrade.terryfactory";
		public override string Description => "Unlocks new technologies and units.";
		public override string ChangeItemTo => "building.terryfactory2";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/terryfactory.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 150,
			[ResourceType.Plasma] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.researchlab"
		};
	}
}
