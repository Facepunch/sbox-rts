using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Medic : BaseUnit, IInfantryUnit
	{
		public override string Name => "Medic";
		public override string UniqueId => "unit.medic";
		public override string Description => "A special Terry that can heal nearby units.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/naked.png" );
		public override int BuildTime => 30;
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Support", Color.Green )
		};
		public override OccupantSettings Occupant => new()
		{
			CanAttack = false
		};
		public override HashSet<string> Abilities => new()
		{
			"ability_heal"
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = -0.2f
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
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 25
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.syringes"
		};
		public override HashSet<string> Clothing => new()
		{
			CitizenClothing.Shoes.Trainers,
			CitizenClothing.Trousers.Jeans,
			CitizenClothing.Jacket.Red,
			CitizenClothing.Hat.Cap
		};
	}
}
