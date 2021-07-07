using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Syringes : BaseTech
	{
		public override string Name => "Syringes";
		public override string UniqueId => "tech.syringes";
		public override string Description => "Useful for getting stuff into a body fast.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
