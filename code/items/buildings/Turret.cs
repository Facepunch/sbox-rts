using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Turret : BaseBuilding
	{
		public override string Name => "Turret";
		public override string UniqueId => "building.turret";
		public override string Description => "Base defense structure that automatically fires upon enemy ground units.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/turret.png" );
		public override int BuildTime => 20;
		public override float MaxHealth => 400f;
		public override float AttackRadius => 1200f;
		public override float MinLineOfSight => 1200f;
		public override float MaxVerticalRange => 150f;
		public override string Weapon => "weapon_turret";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/turret/turret.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre",
			"tech.machinery"
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.bullet"] = 0.3f,
			["resistance.fire"] = 0.1f
		};
	}
}
