using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Cryomaniac : BaseUnit
	{
		public override string Name => "Cryomaniac";
		public override string UniqueId => "unit.cryomaniac";
		public override string Description => "This Terry loves freezing others to slow them down. And death, he loves that too.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override float MinAttackDistance => 500f;
		public override float MaxHealth => 125f;
		public override int BuildTime => 20;
		public override OccupantSettings Occupant => new()
		{
			CanAttack = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 50
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
		public override string Weapon => "weapon_icethrower";
		public override HashSet<string> Dependencies => new()
		{
			"tech.cryogenics"
		};
		public override HashSet<string> Clothing => new()
		{
			CitizenClothing.Shoes.Trainers,
			CitizenClothing.Trousers.Jeans,
			CitizenClothing.Jacket.Red,
			CitizenClothing.Hat.TopHat
		};
	}
}
