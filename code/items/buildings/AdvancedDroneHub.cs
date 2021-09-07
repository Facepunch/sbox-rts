using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedDroneHub : DroneHub
	{
		public override string Name => "Advanced Drone Hub";
		public override string UniqueId => "building.dronehub2";
		public override string[] ActsAsProxyFor => new string[] { "building.dronehub" };
		public override string Model => "models/buildings/drone_hub/drone_hub_level2.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"unit.bubbledrone",
			"unit.suicidedrone",
			"tech.graphitearmor",
			"tech.aerodynamics"
		};
	}
}
