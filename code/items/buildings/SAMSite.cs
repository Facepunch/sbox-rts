using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class SAMSite : BaseBuilding
	{
		public override string Name => "SAM Site";
		public override string UniqueId => "building.samsite";
		public override string Description => "Base defense structure that automatically fires upon enemy aircraft.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/samsite.png" );
		public override int BuildTime => 30;
		public override float MaxHealth => 300f;
		public override float AttackRadius => 1300f;
		public override float MaxVerticalRange => 1000f;
		public override float MinVerticalRange => 300f;
		public override float MinLineOfSight => 1300f;
		public override string Weapon => "weapon_sam";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/sam_site/sam_site.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre2",
			"tech.infrastructure"
		};
	}
}
