using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Worker : BaseUnit
	{
		public override string Name => "Worker";
		public override string UniqueId => "unit.worker";
		public override Texture Icon => Texture.Load( "textures/rts/icons/worker.png" );
		public override bool CanConstruct => true;
		public override bool CanEnterBuildings => true;
		public override string Description => "Gathers Wood, Stone and Beer for your empire and constructs buildings.";
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
		public override int BuildTime => 1;
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
		public override HashSet<string> Buildables => new()
		{
			"building.headquarters",
			"building.brewery",
			"building.pub",
			"building.silo",
			"building.turret",
			"building.terryfactory",
			"building.vehiclefactory"
		};
		public override HashSet<string> Clothing => new()
		{
			"models/citizen_clothes/hat/hat_hardhat.vmdl"
		};
	}
}
