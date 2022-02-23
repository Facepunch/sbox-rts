using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Boring : BaseTech
	{
		public override string Name => "Boring";
		public override string UniqueId => "tech.boring";
		public override string Description => "Unlock the secrets of the underground.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.machinery",
			"tech.infrastructure"
		};
	}
}
