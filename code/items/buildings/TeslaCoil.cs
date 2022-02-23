using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class TeslaCoil : BaseBuilding
	{
		public override string Name => "Tesla Coil";
		public override string UniqueId => "building.teslacoil";
		public override string Description => "Base defense structure that evenly distributes damage to targets around it.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/teslacoil.png" );
		public override int BuildTime => 30;
		public override float MaxHealth => 300f;
		public override float AttackRadius => 1200f;
		public override string Weapon => "weapon_tesla_coil";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/tesla_coil/tesla_coil.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"tech.overvoltage"
		};
	}
}
