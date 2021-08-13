using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class SupplyLines : BaseTech
	{
		public override string Name => "Supply Lines";
		public override string UniqueId => "tech.supplylines";
		public override string Description => "Unlocks the Trading Centre to exchange resources.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure"
		};
	}
}
