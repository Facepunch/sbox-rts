using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class AdvancedAirfield : Airfield
	{
		public override string Name => "Advanced Airfield";
		public override string UniqueId => "building.airfield2";
		public override string[] ActsAsProxyFor => new string[] { "building.airfield" };
		public override string Model => "models/buildings/airfield/airfield_level2.vmdl";
		public override HashSet<string> Queueables => new( base.Queueables )
		{
			"unit.apache"
		};
	}
}
