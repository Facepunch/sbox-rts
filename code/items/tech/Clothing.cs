using Sandbox;
using System.Collections.Generic;

namespace RTS.Tech
{
	[Library]
	public class Clothing : BaseTech
	{
		public override string Name => "Clothing";
		public override string UniqueId => "tech.clothing";
		public override string Description => "Unlocks some new, clad units for your empire.";
		public override int BuildTime => 120;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200
		};
	}
}
