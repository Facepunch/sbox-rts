using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedCommandCentre : CommandCentre
	{
		public override string Name => "Advanced Command Centre";
		public override string UniqueId => "building.commandcentre2";
		public override HashSet<string> Abilities => new()
		{
			"ability_radar_scan"
		};
		public override string[] ActsAsProxyFor => new string[] { "building.commandcentre" };
		public override string Model => "models/buildings/headquarters/headquarters_level2.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"upgrade.commandcentre2"
		};
	}
}
