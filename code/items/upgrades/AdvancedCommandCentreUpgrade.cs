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
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/headquarters.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 300,
			[ResourceType.Metal] = 300
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.researchlab"
		};
	}
}
