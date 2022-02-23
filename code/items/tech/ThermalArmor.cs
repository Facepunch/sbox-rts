using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class ThermalArmor : ResistanceModifierTech<BaseUnit>
	{
		public override string Name => "Thermal Armor";
		public override string UniqueId => "tech.thermalarmor";
		public override string Description => "Increases the fire resistance of all units by 10%.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override Dictionary<string,float> ResistanceModifiers => new()
		{
			["resistance.fire"] = 0.1f
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
	}
}
