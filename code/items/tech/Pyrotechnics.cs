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
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 100,
			[ResourceType.Plasma] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.carbines",
			"tech.extraction"
		};
	}
}
