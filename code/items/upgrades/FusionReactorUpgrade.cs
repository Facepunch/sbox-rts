using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class FusionReactorUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Fusion Reactor";
		public override string UniqueId => "upgrade.fusionreactor";
		public override string Description => "Upgrade to produce Plasma at twice the rate.";
		public override string ChangeItemTo => "building.advancedfusionreactor";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/fusionreactor.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.darkenergy"
		};
	}
}
