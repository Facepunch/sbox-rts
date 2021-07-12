using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Carbines : BaseTech
	{
		public override string Name => "Carbines";
		public override string UniqueId => "tech.carbines";
		public override string Description => "Unlocks new and wonderful firearms for your arsenal.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
