using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Assault : BaseUnit, IInfantryUnit
	{
		public override string Name => "Assault";
		public override string UniqueId => "unit.assault";
		public override string Description => "A more agile Terry equipped with an assault rifle.";
		public override float Speed => 350f;
		public override float MaxVerticalRange => 650f;
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Attacks Aircraft" )
		};
		public override Texture Icon => Texture.Load( "textures/rts/icons/assault.png" );
		public override int BuildTime => 15;
		public override OccupantSettings Occupant => new()
		{
			CanAttack = true
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_adrenaline"
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 75
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
		public override string Weapon => "weapon_smg";
		public override HashSet<string> Dependencies => new()
		{
			
		};
		public override HashSet<string> Queueables => new()
		{
			"upgrade.plasmaassault"
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
