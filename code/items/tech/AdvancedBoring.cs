using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class AdvancedBoring : BaseTech
	{
		public override string Name => "Advanced Boring";
		public override string UniqueId => "tech.advancedboring";
		public override string Description => "Unlocks upgrades for resource drills.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override bool AlwaysShowInList => false;
		public override int BuildTime => 80;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.boring"
		};
	}
}
