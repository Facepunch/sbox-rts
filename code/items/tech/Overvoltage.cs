using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Overvoltage : BaseTech
	{
		public override string Name => "Overvoltage";
		public override string UniqueId => "tech.overvoltage";
		public override string Description => "Harness the power of electricity to combat your foes.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
	}
}
