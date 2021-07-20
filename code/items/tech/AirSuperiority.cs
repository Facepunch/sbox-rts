using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class AirSuperiority : BaseTech
	{
		public override string Name => "Air Superiority";
		public override string UniqueId => "tech.airsuperiority";
		public override string Description => "The ability to fly can unleash great new military potential.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
	}
}
