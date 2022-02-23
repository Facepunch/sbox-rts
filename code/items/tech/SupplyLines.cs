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
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure"
		};
	}
}
