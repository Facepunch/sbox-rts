using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class FusionReactor : BaseBuilding
	{
		public override string Name => "Fusion Reactor";
		public override string UniqueId => "building.fusionreactor";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/fusionreactor.png" );
		public override string Description => "Assign up to 4 scientists to generate Plasma over time.";
		public override float MaxHealth => 600f;
		public override int BuildTime => 30;
		public override OccupiableSettings Occupiable => new()
		{
			Whitelist = new() { "unit.scientist" },
			MaxOccupants = 4,
			Enabled = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 400,
			[ResourceType.Metal] = 600
		};
		public override string Model => "models/buildings/fusion_reactor/fusion_reactor.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre2",
			"tech.extraction"
		};
		public override ResourceGenerator Generator => new()
		{
			PerOccupant = true,
			Resources = new() {{ ResourceType.Plasma, 1 }},
			FinishSound = "rts.generator.collect1",
			LoopSound = "rts.generator.drillingloop",
			Interval = 20f
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.fusionreactor"
		};
	}
}
