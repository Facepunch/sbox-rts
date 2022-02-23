using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Ranger : BaseUnit, IVehicleUnit
	{
		public override string Name => "Ranger";
		public override string UniqueId => "unit.ranger";
		public override string Model => "models/vehicles/humvee/humvee.vmdl";
		public override string Description => "A basic vehicle good for scouting large areas.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/vehicles/humvee.png" );
		public override float MaxHealth => 200f;
		public override bool UseRenderColor => true;
		public override bool UseModelPhysics => true;
		public override bool RagdollOnDeath => false;
		public override float AgentRadiusScale => 1.5f;
		public override bool UseBoundsToAlign => true;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override int NodeSize => 50;
		public override int CollisionSize => 100;
		public override float LineOfSightRadius => 1500f;
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
		public override float Speed => 500f;
		public override int BuildTime => 20;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50,
			[ResourceType.Metal] = 75
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.explosive"] = -0.3f,
			["resistance.bullet"] = 0.3f,
			["resistance.fire"] = 0.2f
		};
	}
}
