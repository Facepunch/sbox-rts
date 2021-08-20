using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class MegaCommandCentre : AdvancedCommandCentre
	{
		public override string Name => "Mega Command Centre";
		public override string UniqueId => "building.commandcentre3";
		public override HashSet<string> Abilities => new( base.Abilities )
		{
			"ability_airstrike"
		};
		public override string Model => "models/buildings/headquarters/headquarters_level3.vmdl";
	}
}
