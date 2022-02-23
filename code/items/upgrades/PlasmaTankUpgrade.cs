using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class PlasmaTankUpgrade : BaseWeaponUpgrade
	{
		public override string Name => "Plasma Cannon";
		public override string UniqueId => "upgrade.plasmatank";
		public override string Description => "Upgrade to a cannon that deals Plasma splash damage.";
		public override string ChangeWeaponTo => "weapon_plasma_tank_cannon";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/stonedrill.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 20
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.darkenergy"
		};
	}
}
