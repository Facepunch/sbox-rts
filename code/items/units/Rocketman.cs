using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Rocketman : BaseUnit, IInfantryUnit
	{
		public override string Name => "Rocketman";
		public override string UniqueId => "unit.rocketman";
		public override string Description => "A slow Terry armed with a rocket launcher. Can attack aircraft.";
		public override float Speed => 350f;
		public override float MaxVerticalRange => 650f;
		public override float MaxHealth => 125f;
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override int BuildTime => 20;
		public override OccupantSettings Occupant => new()
		{
			CanAttack = true
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 75,
			[ResourceType.Metal] = 25,
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = -0.2f
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
		public override string Weapon => "weapon_rocket_launcher";
		public override HashSet<string> Dependencies => new()
		{
			"tech.pyrotechnics"
		};
		public override HashSet<string> Clothing => new()
		{
			CitizenClothing.Shoes.WorkBoots,
			CitizenClothing.Trousers.Police,
			CitizenClothing.Shirt.Longsleeve.Plain,
			CitizenClothing.Jacket.Heavy,
			//CitizenClothing.Vest.Kevlar,
			CitizenClothing.Hat.SecurityHelmet.Normal
		};
	}
}
