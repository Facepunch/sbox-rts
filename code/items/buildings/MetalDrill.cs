﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class MetalDrill : BaseBuilding
	{
		public override string Name => "Metal Drill";
		public override string UniqueId => "building.metaldrill";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/metaldrill.png" );
		public override string Description => "Assign up to 4 workers to generate Metal over time.";
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
		public override string Model => "models/buildings/metal_drill/metal_drill.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"tech.boring"
		};
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Metal, 2 }},
			Interval = 10f
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.metaldrill"
		};
	}
}
