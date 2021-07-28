using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class LaunchPad : BaseBuilding
	{
		public override string Name => "Launch Pad";
		public override string UniqueId => "building.launchpad";
		public override string Description => "A structure capable of launching a nuclear weapon.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/vehiclefactory.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 500,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/nuke_launch_site/nuke_launch_site.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			//"tech.armageddon"
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_nuke"
		};
	}
}
