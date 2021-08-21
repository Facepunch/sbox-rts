using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class PlasmaAssaultUpgrade : BaseWeaponUpgrade
	{
		public override string Name => "Plasma Weapon";
		public override string UniqueId => "upgrade.plasmaassault";
		public override string Description => "Upgrade to a weapon that deals Plasma damage.";
		public override string ChangeWeaponTo => "weapon_plasma_smg";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/stonedrill.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 10
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.darkenergy"
		};
	}
}
