using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Extraction : BaseTech
	{
		public override string Name => "Extraction";
		public override string UniqueId => "tech.extraction";
		public override string Description => "Unlocks the ability to extract plasma with Scientists.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/clothing.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.infrastructure"
		};
	}
}
