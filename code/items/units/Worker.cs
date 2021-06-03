using Sandbox;
using System.Collections.Generic;

namespace RTS.Units
{
	[Library]
	public class Worker : BaseUnit
	{
		public override string Name => "Worker";
		public override string UniqueId => "unit.worker";
		public override string Description => "Gathers Wood, Stone and Beer for your empire and constructs buildings.";
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50
		};
	}
}
