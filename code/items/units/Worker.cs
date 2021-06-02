using Sandbox;

namespace RTS.Units
{
	[Library]
	public class Worker : BaseUnit
	{
		public override string Name => "Worker";
		public override string UniqueId => "unit.gatherer";
		public override string Description => "Gathers Wood, Stone and Beer for your empire and constructs buildings.";
		public override int BuildTime => 30;
		public override ResourceType Resource => ResourceType.Beer;
		public override int Cost => 50;
	}
}
