using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class AttackDrone : BaseUnit
	{
		public override string Name => "Attack Drone";
		public override string UniqueId => "unit.attackdrone";
		public override string Entity => "unit_drone";
		public override Texture Icon => Texture.Load( "textures/rts/icons/worker.png" );
		public override string Description => "A fast but weak combat drone to deliver pain from the sky.";
		public override string Model => "models/vehicles/drones/attack/drone_attack.vmdl";
		public override string Weapon => "weapon_attack_drone";
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
			"brute.alright",
			"brute.move_it",
			"brute.search_and_destroy",
			"brute.take_em_down"
		};
		public override string[] MoveSounds => new string[]
		{
			"brute.alright_move_out",
			"brute.as_you_wish",
			"brute.going_there",
			"brute.got_it",
			"brute.lets_do_this",
			"brute.lets_get_it_done"
		};
		public override string[] SelectSounds => new string[]
		{
			"brute.ready",
			"brute.ready2",
			"brute.tell_me_what_to_do",
			"brute.tell_me_what_to_do2",
			"brute.yes_boss"
		};
		public override int BuildTime => 1;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
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
			"tech.carbines"
		};
	}
}
