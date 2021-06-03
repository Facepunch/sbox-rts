using Sandbox;
using System.Collections.Generic;

namespace RTS.Tech
{
	[Library]
	public class Wheels : BaseTech
	{
		public override string Name => "Wheels";
		public override string UniqueId => "tech.wheels";
		public override string Description => "Everything changes once you can move on wheels.";
		public override int BuildTime => 300;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
