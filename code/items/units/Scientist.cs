using System.Collections.Generic;
using Sandbox;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Scientist : BaseUnit
	{
		public override string Name => "Scientist";
		public override string UniqueId => "unit.scientist";
		public override string Description => "Extracts plasma for advanced technology, constructions and units.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/scientist.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
		public override HashSet<ResourceType> Gatherables => new()
		{
			ResourceType.Plasma
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.clothing"
		};
		public override HashSet<string> Clothing => new()
		{
			"models/citizen_clothes/jacket/labcoat.vmdl",
			"models/citizen_clothes/gloves/gloves_workgloves.vmdl",
			"models/citizen_clothes/trousers/trousers.lab.vmdl",
			"models/citizen_clothes/shoes/shoes.workboots.vmdl",
		};
	}
}
