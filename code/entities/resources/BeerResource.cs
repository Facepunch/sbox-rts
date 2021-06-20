using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "resource_beer" )]
	[Hammer.Model( Model = "models/barrels/square_wooden_box_gold.vmdl" )]
	public partial class BeerResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Beer;
		public override string Description => "You can find Beer for your empire in this cache.";
		public override string Name => "Beer Cache";
	}
}
