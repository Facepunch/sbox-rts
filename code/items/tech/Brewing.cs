using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Brewing : BaseTech
	{
		public override string Name => "Brewing";
		public override string UniqueId => "tech.brewing";
		public override string Description => "Unlocks a revolutionary new way of generating beer.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/brewing.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure"
		};
	}
}
