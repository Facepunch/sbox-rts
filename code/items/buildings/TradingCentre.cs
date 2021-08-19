using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class TradingCentre : BaseBuilding
	{
		public override string Name => "Trading Centre";
		public override string UniqueId => "building.tradingcentre";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/tradingcentre.png" );
		public override string Description => "You can trade resources from this building.";
		public override float MaxHealth => 1000f;
		public override float MinLineOfSight => 500f;
		public override int BuildTime => 40;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 500,
			[ResourceType.Metal] = 300
		};
		public override string Model => "models/buildings/trading_centre/trading_centre.vmdl";
		public override HashSet<string> Abilities => new()
		{
			"ability_sell_stone",
			"ability_sell_metal",
			"ability_buy_stone",
			"ability_buy_metal"
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.supplylines"
		};
	}
}
