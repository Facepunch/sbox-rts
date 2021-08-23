using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "resource_stone" )]
	[Hammer.Model( Model = "models/rocks/rock_large_00.vmdl", MaterialGroup = "Rock" )]
	public partial class StoneResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Stone;
		public override int DefaultStock => 400;
		public override string Description => "You can mine this to gather Stone for your empire.";
		public override string Name => "Rocks";
	}
}
