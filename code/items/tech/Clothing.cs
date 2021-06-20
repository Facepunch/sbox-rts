using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Clothing : BaseTech
	{
		public override string Name => "Clothing";
		public override string UniqueId => "tech.clothing";
		public override string Description => "Unlocks some new, clad units for your empire.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/clothing.png" );
		public override int BuildTime => 20;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200
		};
	}
}
