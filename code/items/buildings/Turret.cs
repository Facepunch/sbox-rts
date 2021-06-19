using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class Turret : BaseBuilding
	{
		public override string Name => "Turret";
		public override string UniqueId => "building.turret";
		public override string Description => "Base defense structure that automatically fires upon invaders.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/silo.png" );
		public override int BuildTime => 10;
		public override float AttackRange => 1500f;
		public override string Weapon => "weapon_turret_gun";
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/turret_future/turret.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters"
		};
	}
}
