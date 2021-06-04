using Sandbox;
using System.Collections.Generic;

namespace RTS.Units
{
	[Library]
	public class Assault : BaseUnit
	{
		public override string Name => "Assault";
		public override string UniqueId => "unit.assault";
		public override string Description => "A basic Terry armed with only a pistol.";
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
		public override string Weapon => "weapon_pistol";
	}
}
