﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class ArmoredPlating : BaseTech
	{
		public override string Name => "Armored Plating";
		public override string UniqueId => "tech.armoredplating";
		public override string Description => "Unlocks new armored vehicles.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.machinery"
		};
	}
}
