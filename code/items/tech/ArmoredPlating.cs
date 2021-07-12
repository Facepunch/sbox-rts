using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class ArmoredPlating : BaseTech
	{
		public override string Name => "Armored Plating";
		public override string UniqueId => "tech.armoredplating";
		public override string Description => "Useful for surrounding vehicles with armor.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
