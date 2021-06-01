using Sandbox;

namespace RTS.Units
{
	[Library]
	public class Naked : BaseUnit
	{
		public override string Name => "Naked";
		public override string UniqueId => "unit.naked";
		public override string Description => "An angry Terry who seeks only one thing: blood!";
		public override int BuildTime => 60;
		public override ResourceType Resource => ResourceType.Beer;
		public override int Cost => 150;
	}
}
