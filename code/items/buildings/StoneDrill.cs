using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class StoneDrill : BaseBuilding
	{
		public override string Name => "Stone Drill";
		public override string UniqueId => "building.stonedrill";
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewery.png" );
		public override string Description => "Assign up to 4 workers to generate Stone over time.";
		public override float MaxHealth => 250f;
		public override int BuildTime => 5;
		public override OccupiableSettings Occupiable => new()
		{
			Whitelist = new() { "unit.worker" },
			MaxOccupants = 4,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 200
		};
		public override string Model => "models/buildings/brewery/brewery.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"tech.boring"
		};
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Stone, 4 }},
			Interval = 10f
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.stonedrill"
		};
	}
}
