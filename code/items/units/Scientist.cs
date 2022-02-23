using System.Collections.Generic;
using Gamelib.Utility;
using Sandbox;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Scientist : BaseUnit, IInfantryUnit
	{
		public override string Name => "Scientist";
		public override string UniqueId => "unit.scientist";
		public override string Description => "Extracts plasma for advanced technology, constructions and units.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/scientist.png" );
		public override int BuildTime => 10;
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Gatherer", Color.Orange )
		};
		public override OccupantSettings Occupant => new()
		{
			CanAttack = false
		};
		public override string[] MoveSounds => new string[]
		{
			"scientist.be_there_soon",
			"scientist.going_there_now"
		};
		public override string[] DepositSounds => MoveSounds;
		public override string[] SelectSounds => new string[]
		{
			"scientist.tell_me_what_to_do",
			"scientist.ready",
			"scientist.ready2",
			"scientist.ready_to_extract"
		};
		public override Dictionary<ResourceType, string[]> GatherSounds => new()
		{
			[ResourceType.Plasma] = new string[]
			{
				"scientist.going_to_get_it",
				"scientist.extracting_plasma",
				"scientist.extracting_plasma2"
			}
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Plasma
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = -0.2f
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.extraction"
		};
		public override HashSet<string> Clothing => new()
		{
			CitizenClothing.Shoes.SmartBrown,
			CitizenClothing.Trousers.Lab,
			CitizenClothing.Shirt.Longsleeve.Scientist
		};
	}
}
