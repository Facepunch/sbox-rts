using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Infrastructure : BaseTech
	{
		public override string Name => "Infrastructure";
		public override string UniqueId => "tech.infrastructure";
		public override string Description => "Learn to create order from the chaos of the natural world.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.machinery"
		};
	}
}
