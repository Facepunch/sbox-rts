using Sandbox;
using System.Collections.Generic;

namespace RTS.Buildings
{
	[Library]
	public class Brewery : BaseBuilding
	{
		public override string Name => "Brewery";
		public override string UniqueId => "building.brewery";
		public override Texture Icon => Texture.Load( "textures/rts/icons/brewery.png" );
		public override string Description => "Assign up to 4 workers to generate Beer over time.";
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 200
		};
		public override string Model => "models/buildings/brewery_future/brewery.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters",
			"tech.brewing"
		};
	}
}
