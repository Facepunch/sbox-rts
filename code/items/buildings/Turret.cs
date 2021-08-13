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
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/turret.png" );
		public override int BuildTime => 10;
		public override float MaxHealth => 250f;
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
			"building.headquarters",
			"tech.machinery"
		};
	}
}
