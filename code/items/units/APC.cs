using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class APC : BaseUnit, IVehicleUnit
	{
		public override string Name => "APC";
		public override string UniqueId => "unit.apc";
		public override string Model => "models/vehicles/apc/apc.vmdl";
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Transport", Color.Cyan )
		};
		public override string Description => "An armored vehicle for transporting units long distance.";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/vehicles/apc.png" );
		public override bool UseRenderColor => true;
		public override bool UseModelPhysics => true;
		public override float MaxHealth => 250f;
		public override int NodeSize => 50;
		public override int CollisionSize => 100;
		public override bool RagdollOnDeath => false;
		public override float AgentRadiusScale => 1.5f;
		public override bool UseBoundsToAlign => true;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override float LineOfSightRadius => 1500f;
		public override OccupiableSettings Occupiable => new()
		{
			MaxOccupants = 4,
			DamageScale = 0.2f,
			Enabled = true
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
		public override float Speed => 500f;
		public override int BuildTime => 25;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 75,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{

		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.explosive"] = -0.3f,
			["resistance.bullet"] = 0.6f,
			["resistance.fire"] = 0.2f
		};
	}
}
