﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Boring : BaseTech
	{
		public override string Name => "Boring";
		public override string UniqueId => "tech.boring";
		public override string Description => "Unlock the secrets of the underground.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.machinery",
			"tech.infrastructure"
		};
	}
}
