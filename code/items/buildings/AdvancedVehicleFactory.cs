using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedVehicleFactory : VehicleFactory
	{
		public override string Name => "Advanced Vehicle Factory";
		public override string UniqueId => "building.vehiclefactory2";
		public override string[] ActsAsProxyFor => new string[] { "building.vehiclefactory" };
		public override string Model => "models/buildings/vehiclefactory/vehiclefactory_level2.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"unit.apc",
			"unit.tank"
		};
	}
}
