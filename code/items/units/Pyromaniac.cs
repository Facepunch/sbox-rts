using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Pyromaniac : BaseUnit, IInfantryUnit
	{
		public override string Name => "Pyromaniac";
		public override string UniqueId => "unit.pyromaniac";
		public override string Description => "This Terry loves fire. A little too much, perhaps.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/assault.png" );
		public override float MinAttackDistance => 500f;
		public override int BuildTime => 20;
		public override float MaxHealth => 125f;
		public override OccupantSettings Occupant => new()
		{
			CanAttack = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 125,
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
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = 0.2f
		};
		public override string Weapon => "weapon_flamethrower";
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
		public override HashSet<string> Clothing => new()
		{
			"trainers",
			"baseball_cap",
			"hoodie",
			"tactical_vest",
			"trousers.smart"
		};
	}
}
