using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Medevac : BaseUnit
	{
		public override string Name => "Medevac";
		public override string UniqueId => "unit.medevac";
		public override string Entity => "unit_aircraft";
		public override string Weapon => "weapon_medevac";
		public override string Model => "models/vehicles/medevac/medevac.vmdl";
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Support", Color.Green ),
			new ItemLabel( "Transport", Color.Cyan)
		};
		public override string Description => "An aircraft that can evacuate two units and passively heal nearby targets.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/vehicles/medevac.png" );
		public override float VerticalOffset => 600f;
		public override float MaxHealth => 300f;
		public override bool UsePathfinder => false;
		public override bool UseRenderColor => true;
		public override bool UseModelPhysics => true;
		public override bool RagdollOnDeath => false;
		public override float AgentRadiusScale => 1.5f;
		public override float RotateToTargetSpeed => 20f;
		public override bool AlignToSurface => false;
		public override string IdleLoopSound => "rts.helicopterloop";
		public override float AttackRadius => 1000f;
		public override float MinAttackDistance => 500f;
		public override float MaxVerticalRange => 1000f;
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override float LineOfSightRadius => 2000f;
		public override OccupiableSettings Occupiable => new()
		{
			MaxOccupants = 2,
			DamageScale = 0.3f,
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
		public override float Speed => 700f;
		public override int BuildTime => 30;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 125
		};
		public override HashSet<string> Dependencies => new()
		{
			
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.explosive"] = -0.4f,
			["resistance.bullet"] = 0.4f,
			["resistance.fire"] = 0.15f
		};
	}
}
