using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class MegaCommandCentreUpgrade : BaseUpgrade
	{
		public override string Name => "Mega Command Centre";
		public override string UniqueId => "upgrade.commandcentre2";
		public override string Description => "Unlocks new abilities and structures.";
		public override string ChangeItemTo => "building.commandcentre3";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/headquarters.png" );
		public override int BuildTime => 80;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 400,
			[ResourceType.Metal] = 400,
			[ResourceType.Plasma] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.advancedresearchlab"
		};
	}
}
