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
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/nukelaunch.png" );
		public override int BuildTime => 70;
		public override float MaxHealth => 800f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 500,
			[ResourceType.Metal] = 300,
		};
		public override string Model => "models/buildings/nuke_launch_site/nuke_launch_site.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre3",
			"tech.armageddon"
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_nuke"
		};
	}
}
