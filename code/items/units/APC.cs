using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class APC : BaseUnit
	{
		public override string Name => "APC";
		public override string UniqueId => "unit.apc";
		public override string Model => "models/vehicles/apc/apc.vmdl";
		public override string Description => "An armored vehicle for transporting units long distance.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/scout.png" );
		public override bool UseRenderColor => true;
		public override bool UseModelPhysics => true;
		public override int NodeSize => 100;
		public override int LineOfSightRadius => 1500;
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
			["resistance.fire"] = 0.2f
		};
	}
}
