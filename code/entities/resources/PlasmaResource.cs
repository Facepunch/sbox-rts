using Sandbox;
using System;

namespace RTS
{
	[Library( "resource_plasma" )]
	[Hammer.Model( Model = "models/rocks/rock_large_00.vmdl", MaterialGroup = "Plasma" )]
	public class PlasmaResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Plasma;
		public override string Description => "You can mine this to gather Plasma for your empire.";
		public override string Name => "Plasma";
	}
}
