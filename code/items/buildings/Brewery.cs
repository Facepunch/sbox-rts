using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Brewery : BaseBuilding
	{
		public override string Name => "Brewery";
		public override string UniqueId => "building.brewery";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/brewery.png" );
		public override string Description => "Assign up to 4 workers to generate Beer over time.";
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
		public override string Model => "models/buildings/brewery/brewery.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre",
			"tech.brewing"
		};
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Beer, 5 }},
			FinishSound = "rts.brewery.collect",
			LoopSound = "rts.brewery.loop",
			Interval = 10f
		};
	}
}
