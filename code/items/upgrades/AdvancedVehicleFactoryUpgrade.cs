using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class AdvancedVehicleFactoryUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Vehicle Factory";
		public override string UniqueId => "upgrade.vehiclefactory";
		public override string Description => "Unlocks new technologies and units.";
		public override string ChangeItemTo => "building.vehiclefactory2";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/vehiclefactory.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 150,
			[ResourceType.Plasma] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre2",
			"building.researchlab"
		};
	}
}
