using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_sell_metal" )]
	public class SellMetalAbility : BaseTradeAbility
	{
		public override string Name => "Sell Metal";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/heal.png" );
		public override ResourceType Resource => ResourceType.Beer;
		public override int Amount => 75;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 100
		};
	}
}
