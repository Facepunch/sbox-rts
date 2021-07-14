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
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewing.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 300
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure"
		};
	}
}
