using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Cryogenics : BaseTech
	{
		public override string Name => "Cryogenics";
		public override string UniqueId => "tech.cryogenics";
		public override string Description => "Master the art of using cold temperature to your advantage.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 100
		};
	}
}
