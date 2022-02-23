using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class ElectricAttackDroneUpgrade : BaseWeaponUpgrade
	{
		public override string Name => "Electric Weapon";
		public override string UniqueId => "upgrade.electricattackdrone";
		public override string Description => "Upgrade to a weapon that deals Electric damage.";
		public override string ChangeWeaponTo => "weapon_electric_attack_drone";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/stonedrill.png" );
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
