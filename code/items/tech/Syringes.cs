using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Syringes : BaseTech
	{
		public override string Name => "Syringes";
		public override string UniqueId => "tech.syringes";
		public override string Description => "Unlocks the Medic and new abilities.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50,
			[ResourceType.Metal] = 50
		};
	}
}
