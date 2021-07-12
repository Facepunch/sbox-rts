using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Pyrotechnics : BaseTech
	{
		public override string Name => "Pyrotechnics";
		public override string UniqueId => "tech.pyrotechnics";
		public override string Description => "Learn how to use fire and explosions to your advantage.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.carbines"
		};
	}
}
