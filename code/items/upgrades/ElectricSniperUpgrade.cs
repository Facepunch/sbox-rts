using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class ElectricSniperUpgrade : BaseWeaponUpgrade
	{
		public override string Name => "Electric Weapon";
		public override string UniqueId => "upgrade.electricsniper";
		public override string Description => "Upgrade to a weapon that deals Electric damage.";
		public override string ChangeWeaponTo => "weapon_electric_sniper";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/stonedrill.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 50
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.overvoltage"
		};
	}
}
