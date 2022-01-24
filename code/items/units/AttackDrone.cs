using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class AttackDrone : BaseUnit, IDroneUnit
	{
		public override string Name => "Attack Drone";
		public override string UniqueId => "unit.attackdrone";
		public override string Entity => "unit_drone";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/vehicles/drone_attack.png" );
		public override string Description => "A fast but weak combat drone to deliver pain from the sky.";
		public override string Model => "models/vehicles/drones/attack/drone_attack.vmdl";
		public override string Weapon => "weapon_attack_drone";
		public override float MaxHealth => 75f;
		public override HashSet<string> Tags => new() { "drone" };
		public override float VerticalOffset => 100f;
		public override bool UseModelPhysics => true;
		public override float CircleScale => 0.5f;
		public override bool UseRenderColor => true;
		public override bool RagdollOnDeath => false;
		public override float Speed => 500f;
		public override bool AlignToSurface => false;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override string[] AttackSounds => new string[]
		{
			"rts.drone.attack1",
			"rts.drone.attack2"
		};
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
		public override int BuildTime => 15;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 100
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f,
			["resistance.bullet"] = 0.1f,
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_dome_shield"
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.electricattackdrone",
			"upgrade.plasmaattackdrone"
		};
		public override HashSet<string> Dependencies => new()
		{

		};
	}
}
