using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class SuicideDrone : BaseUnit
	{
		public override string Name => "Suicide Drone";
		public override string UniqueId => "unit.suicidedrone";
		public override string Entity => "unit_drone";
		public override string Weapon => "weapon_suicide_drone";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/vehicles/drone_dive.png" );
		public override string Description => "A single-use drone that dives into enemies and explodes.";
		public override string Model => "models/vehicles/drones/dive/drone_dive.vmdl";
		public override HashSet<string> Tags => new() { "drone" };
		public override float VerticalOffset => 100f;
		public override bool UseModelPhysics => true;
		public override float CircleScale => 0.5f;
		public override bool UseRenderColor => true;
		public override bool RagdollOnDeath => false;
		public override float Speed => 450f;
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
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Metal] = 200
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
	}
}
