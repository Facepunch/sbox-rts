﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class ResearchLabUpgrade : BaseUpgrade
	{
		public override string Name => "Advanced Research Lab";
		public override string UniqueId => "upgrade.researchlab";
		public override string Description => "Unlocks more advanced and expensive research.";
		public override string ChangeItemTo => "building.advancedresearchlab";
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewing.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 300
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure",
			"tech.extraction",
			"tech.carbines"
		};
	}
}