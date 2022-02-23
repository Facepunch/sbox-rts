using Gamelib.Utility;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Worker : BaseUnit, IInfantryUnit
	{
		public override string Name => "Worker";
		public override string UniqueId => "unit.worker";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/worker.png" );
		public override List<ItemLabel> Labels => new()
		{
			new ItemLabel( "Gatherer", Color.Orange ),
			new ItemLabel( "Builder", Color.Yellow )
		};
		public override float MaxHealth => 75f;
		public override bool CanConstruct => true;
		public override string Description => "Gathers Stone, Metal and Beer for your empire and constructs buildings.";
		public override string[] MoveSounds => new string[]
		{
			"worker.lets_go",
			"worker.on_my_way"
		};
		public override string[] ConstructSounds => MoveSounds;
		public override string[] DepositSounds => MoveSounds;
		public override string[] SelectSounds => new string[]
		{
			"worker.ready",
			"worker.tell_me_what_to_do"
		};
		public override int BuildTime => 8;
		public override OccupantSettings Occupant => new()
		{
			CanAttack = false
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 50
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Stone,
			ResourceType.Metal,
			ResourceType.Beer
		};
		public override Dictionary<string, float> Resistances => new()
		{
			["resistance.fire"] = -0.2f
		};
		public override HashSet<string> Queueables => new()
		{
			"building.commandcentre",
			"building.brewery",
			"building.stonedrill",
			"building.metaldrill",
			"building.fusionreactor",
			"building.pub",
			"building.silo",
			"building.turret",
			"building.teslacoil",
			"building.terryfactory",
			"building.vehiclefactory",
			"building.dronehub",
			"building.tradingcentre",
			"building.tunnel",
			"building.researchlab",
			"building.radarjammer",
			"building.samsite",
			"building.launchpad",
			"building.watchtower",
			"building.airfield",
			"building.pillbox"
		};
		public override HashSet<string> Clothing => new()
		{
			CitizenClothing.Hat.HardHat,
			CitizenClothing.Shoes.WorkBoots,
			//CitizenClothing.Vest.HighVis,
			CitizenClothing.Trousers.Smart.Tan,
			CitizenClothing.Shirt.Longsleeve.Plain
		};
	}
}
