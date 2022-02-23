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
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/tempicons/silo.png" );
		public override bool CanDepositResources => true;
		public override float MaxHealth => 300f;
		public override int BuildTime => 20;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 100
		};
		public override string Model => "models/buildings/silo/silo.vmdl";
		public override HashSet<string> Dependencies => new()
		{
			"building.commandcentre"
		};
	}
}
