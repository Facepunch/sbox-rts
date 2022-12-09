using Sandbox;
using System;
using Editor;

namespace Facepunch.RTS
{
	[Library( "resource_beer" )]
	[Model( Model = "models/barrels/square_wooden_box_gold.vmdl" )]
	[HammerEntity]
	public partial class BeerResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Beer;
		public override int DefaultStock => 500;
		public override string[] GatherSounds => new string[]
		{
			"gatherbeer1",
			"gatherbeer2",
			"gatherbeer3"
		};
		public override string Description => "You can find Beer for your empire in this cache.";
		public override string ResourceName => "Beer Cache";
	}
}
