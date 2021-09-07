using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedTerryFactory : TerryFactory
	{
		public override string Name => "Advanced Terry Factory";
		public override string UniqueId => "building.terryfactory2";
		public override string[] ActsAsProxyFor => new string[] { "building.terryfactory" };
		public override string Model => "models/buildings/terryfactory/terryfactory_level2.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"unit.heavy",
			"unit.grenadier",
			"unit.rocketman",
			"unit.pyromaniac",
			"unit.cryomaniac",
			"tech.advancedkevlar"
		};
	}
}
