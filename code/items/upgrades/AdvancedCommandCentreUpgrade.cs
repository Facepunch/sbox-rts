using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class AdvancedCommandCentreUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Command Centre";
		public override string UniqueId => "upgrade.commandcentre";
		public override string Description => "Unlocks new abilities and structures.";
		public override string ChangeItemTo => "building.commandcentre2";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/headquarters.png" );
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 100,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.researchlab"
		};
	}
}
