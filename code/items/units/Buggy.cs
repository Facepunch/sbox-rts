using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Buggy : BaseUnit
	{
		public override string Name => "Buggy";
		public override string UniqueId => "unit.buggy";
		public override string Model => "models/vehicles/buggy/buggy.vmdl";
		public override string Description => "A fast vehicle that occupies one ranged unit.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/scout.png" );
		public override bool UseModelPhysics => true;
		public override bool UseRenderColor => true;
		public override float RotateToTargetSpeed => 10f;
		public override string Entity => "unit_buggy";
		public override int NodeSize => 100;
		public override float AttackRadius => 1000f;
		public override float LineOfSightRadius => 1000f;
		public override OccupiableSettings Occupiable => new()
		{
			AttackAttachments = new string[] { "muzzle" },
			DamageScale = 0.5f,
			MaxOccupants = 1,
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
		public override float Speed => 400f;
		public override int BuildTime => 2;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 100
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.armoredplating",
			"tech.pyrotechnics"
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f
		};
	}
}
