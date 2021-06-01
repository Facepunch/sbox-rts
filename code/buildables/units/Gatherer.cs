using Sandbox;

namespace RTS.Units
{
	[Library]
	public class Gatherer : BaseUnit
	{
		public override string Name => "Gatherer";
		public override string UniqueId => "unit.gatherer";
		public override string Description => "Gathers Wood, Stone and Beer for your empire.";
		public override int BuildTime => 30;
		public override ResourceType Resource => ResourceType.Beer;
		public override int Cost => 50;
	}
}
