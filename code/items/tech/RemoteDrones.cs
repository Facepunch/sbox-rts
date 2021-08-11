using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class RemoteDrones : BaseTech
	{
		public override string Name => "Remote Drones";
		public override string UniqueId => "tech.remotedrones";
		public override string Description => "Unlocks the frightening capabilities of remotely controlled drones.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
