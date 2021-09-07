using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class AdvancedAirfieldUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Airfield";
		public override string UniqueId => "upgrade.airfield";
		public override string Description => "Unlocks new technologies and units.";
		public override string ChangeItemTo => "building.airfield2";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/airfield.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 150,
			[ResourceType.Metal] = 150,
			[ResourceType.Plasma] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"building.researchlab"
		};
	}
}
