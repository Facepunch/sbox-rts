using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Chinook : BaseUnit
	{
		public override string Name => "Chinook";
		public override string UniqueId => "unit.chinook";
		public override string Entity => "unit_aircraft";
		public override string Model => "models/vehicles/chinook/chinook.vmdl";
		public override string Description => "An armored aircraft for transporting many units.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/scout.png" );
		public override float VerticalOffset => 500f;
		public override bool UsePathfinder => false;
		public override bool UseRenderColor => true;
		public override bool UseModelPhysics => true;
		public override bool RagdollOnDeath => false;
		public override bool AlignToSurface => false;
		public override string IdleLoopSound => "rts.helicopterloop";
		public override string DeathParticles => "particles/weapons/explosion_ground_large/explosion_ground_large.vpcf";
		public override float LineOfSightRadius => 2000f;
		public override OccupiableSettings Occupiable => new()
		{
			MaxOccupants = 8,
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
		public override float Speed => 700f;
		public override int BuildTime => 2;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.armoredplating"
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.explosive"] = -0.3f,
			["resistance.bullet"] = 0.6f,
			["resistance.fire"] = 0.2f
		};
	}
}
