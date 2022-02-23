using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class StoneDrillUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Stone Drill";
		public override string UniqueId => "upgrade.stonedrill";
		public override string Description => "Upgrade to produce Stone at twice the rate.";
		public override string ChangeItemTo => "building.advancedstonedrill";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/stonedrill.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.advancedboring"
		};
	}
}
