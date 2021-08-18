using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class BubbleDrone : BaseUnit
	{
		public override string Name => "Bubble Drone";
		public override string UniqueId => "unit.bubbledrone";
		public override string Entity => "unit_drone";
		public override Texture Icon => Texture.Load( "textures/rts/icons/worker.png" );
		public override string Description => "Can deploy a deployable dome shield to protect units inside.";
		public override string Model => "models/vehicles/drones/shield/drone_shield.vmdl";
		public override HashSet<string> Tags => new() { "drone" };
		public override float VerticalOffset => 100f;
		public override bool UseModelPhysics => true;
		public override float CircleScale => 0.5f;
		public override bool UseRenderColor => true;
		public override bool RagdollOnDeath => false;
		public override float Speed => 450f;
		public override bool AlignToSurface => false;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override string[] MoveSounds => new string[]
		{
			"rts.drone.move1",
			"rts.drone.move2"
		};
		public override string[] SelectSounds => new string[]
		{
			"rts.drone.select2",
			"rts.drone.select3",
			"rts.drone.select4",
			"rts.drone.select5"
		};
		public override int BuildTime => 1;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50,
			[ResourceType.Plasma] = 10
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_dome_shield"
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.extraction"
		};
	}
}
