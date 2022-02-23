using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_buy_metal" )]
	public class BuyMetalAbility : BaseTradeAbility
	{
		public override string Name => "Buy Metal";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/heal.png" );
		public override ResourceType Resource => ResourceType.Metal;
		public override int Amount => 75;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100
		};
	}
}
