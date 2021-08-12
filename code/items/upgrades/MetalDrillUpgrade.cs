﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class MetalDrillUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Metal Drill";
		public override string UniqueId => "upgrade.metaldrill";
		public override string Description => "Upgrade to produce Metal at twice the rate.";
		public override string ChangeItemTo => "building.advancedmetaldrill";
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewing.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 300
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.advancedboring"
		};
	}
}