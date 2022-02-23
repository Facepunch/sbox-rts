using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_sell_stone" )]
	public class SellStoneAbility : BaseTradeAbility
	{
		public override string Name => "Sell Stone";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/heal.png" );
		public override ResourceType Resource => ResourceType.Beer;
		public override int Amount => 50;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 100
		};
	}
}
