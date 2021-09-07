using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Grenadier : BaseUnit, IInfantryUnit
	{
		public override string Name => "Grenadier";
		public override string UniqueId => "unit.grenadier";
		public override string Description => "Launches various grenades at your enemies. Can attack aircraft.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override float MaxVerticalRange => 650f;
		public override int BuildTime => 20;
		public override HashSet<string> Abilities => new()
		{
			"ability_molotov",
			"ability_ice_bomb",
			"ability_emp_bomb",
			"ability_plasma_bomb"
		};
		public override OccupantSettings Occupant => new()
		{
			CanAttack = true
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = -0.2f
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 25
		};
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
		public override string Weapon => "weapon_grenade_launcher";
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
	}
}
