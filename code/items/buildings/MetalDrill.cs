using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class MetalDrill : BaseBuilding
	{
		public override string Name => "Metal Drill";
		public override string UniqueId => "building.metaldrill";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/metaldrill.png" );
		public override string Description => "Assign up to 4 workers to generate Metal over time.";
		public override float MaxHealth => 400f;
		public override int BuildTime => 30;
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
		public override string Model => "models/buildings/metal_drill/metal_drill.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre",
			"tech.boring"
		};
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Metal, 2 }},
			FinishSound = "rts.generator.collect1",
			LoopSound = "rts.generator.drillingloop",
			Interval = 10f
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.metaldrill"
		};
	}
}
