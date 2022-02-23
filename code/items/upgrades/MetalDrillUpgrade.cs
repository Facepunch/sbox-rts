using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class MetalDrillUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Metal Drill";
		public override string UniqueId => "upgrade.metaldrill";
		public override string Description => "Upgrade to produce Metal at twice the rate.";
		public override string ChangeItemTo => "building.advancedmetaldrill";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/metaldrill.png" );
		public override int BuildTime => 60;
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
