using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_buy_stone" )]
	public class BuyStoneAbility : BaseTradeAbility
	{
		public override string Name => "Buy Stone";
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override ResourceType Resource => ResourceType.Stone;
		public override int Amount => 100;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 75
		};
	}
}
