﻿using Sandbox;
using System.Collections.Generic;

namespace RTS.Tech
{
	[Library]
	public class Brewing : BaseTech
	{
		public override string Name => "Brewing";
		public override string UniqueId => "tech.brewing";
		public override string Description => "Unlocks a revolutionary new way of generating beer.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewing.png" );
		public override int BuildTime => 300;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 300
		};
	}
}
