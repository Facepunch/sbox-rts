using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class MinerDrone : BaseUnit, IDroneUnit
	{
		public override string Name => "Miner Drone";
		public override string UniqueId => "unit.minerdrone";
		public override string Entity => "unit_drone";
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Gatherer", Color.Orange )
		};
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/vehicles/drone_worker.png" );
		public override string Description => "Gathers Stone and Metal for your empire.";
		public override string Model => "models/vehicles/drones/worker/drone_worker.vmdl";
		public override HashSet<string> Tags => new() { "drone" };
		public override float VerticalOffset => 100f;
		public override bool UseModelPhysics => true;
		public override bool UseRenderColor => true;
		public override float CircleScale => 0.5f;
		public override bool RagdollOnDeath => false;
		public override int MaxCarryMultiplier => 3;
		public override float Speed => 550f;
		public override bool AlignToSurface => false;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override string[] MoveSounds => new string[]
		{
			"rts.drone.move1",
			"rts.drone.move2"
		};
		public override string[] ConstructSounds => MoveSounds;
		public override string[] DepositSounds => MoveSounds;
		public override string[] SelectSounds => new string[]
		{
			"rts.drone.select2",
			"rts.drone.select3",
			"rts.drone.select4",
			"rts.drone.select5"
		};
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 50
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Stone,
			ResourceType.Metal
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f,
			["resistance.bullet"] = 0.2f
		};
	}
}
