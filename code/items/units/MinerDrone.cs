using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class MinerDrone : BaseUnit
	{
		public override string Name => "Miner Drone";
		public override string UniqueId => "unit.minerdrone";
		public override Texture Icon => Texture.Load( "textures/rts/icons/worker.png" );
		public override string Description => "Gathers Stone and Metal for your empire.";
		public override string Model => "models/vehicles/drones/worker/drone_worker.vmdl";
		public override float VerticalOffset => 150f;
		public override bool UseModelPhysics => true;
		public override bool RagdollOnDeath => false;
		public override int MaxCarryMultiplier => 3;
		public override float Speed => 550f;
		public override bool AlignToSurface => false;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override string[] MoveSounds => new string[]
		{
			"worker.lets_go",
			"worker.on_my_way"
		};
		public override string[] ConstructSounds => MoveSounds;
		public override string[] DepositSounds => MoveSounds;
		public override string[] SelectSounds => new string[]
		{
			"worker.ready",
			"worker.tell_me_what_to_do"
		};
		public override int BuildTime => 1;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Stone,
			ResourceType.Metal
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f
		};
	}
}
