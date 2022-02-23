using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Machinery : BaseTech
	{
		public override string Name => "Machinery";
		public override string UniqueId => "tech.machinery";
		public override string Description => "Unlocks the ability to build machines.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50,
			[ResourceType.Metal] = 50
		};
	}
}
