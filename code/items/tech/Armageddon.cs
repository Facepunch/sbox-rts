using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Armageddon : BaseTech
	{
		public override string Name => "Armageddon";
		public override string UniqueId => "tech.armageddon";
		public override string Description => "Unlock the ultimate weapons of war and annihiliate your enemies.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/wheels.png" );
		public override int BuildTime => 120;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 200,
			[ResourceType.Plasma] = 200
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.darkenergy"
		};
	}
}
