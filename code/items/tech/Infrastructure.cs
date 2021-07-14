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
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.machinery"
		};
	}
}
