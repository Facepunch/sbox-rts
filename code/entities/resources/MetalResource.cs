using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "resource_metal" )]
	[Hammer.Model( Model = "models/rocks/rock_large_00.vmdl", MaterialGroup = "Metal" )]
	public class MetalResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Metal;
		public override int DefaultStock => 300;
		public override string Description => "You can mine this to gather Metal for your empire.";
		public override string ResourceName => "Metal Ore";
	}
}
