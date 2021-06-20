using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Silo : BaseBuilding
	{
		public override string Name => "Silo";
		public override string UniqueId => "building.silo";
		public override string Description => "Acts as a deposit point for resources.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/silo.png" );
		public override bool CanDepositResources => true;
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 200,
			[ResourceType.Metal] = 100
		};
		public override string Model => "models/buildings/silo_future/silo.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.headquarters"
		};
	}
}
