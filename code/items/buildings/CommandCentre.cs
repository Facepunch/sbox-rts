using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class CommandCentre : BaseBuilding
	{
		public override string Name => "Command Centre";
		public override string UniqueId => "building.commandcentre";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/tempicons/headquarters.png" );
		public override string Description => "This is the heart of your empire. Protect it at all costs.";
		public override bool CanDepositResources => true;
		public override float MaxHealth => 2000f;
		public override float MinLineOfSight => 500f;
		public override bool BuildFirstInstantly => true;
		public override int BuildTime => 60;
		public override bool CanDemolish => true;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 600,
			[ResourceType.Metal] = 300
		};
		public override string Model => "models/buildings/headquarters/headquarters.vmdl";
		public override HashSet<string> Queueables => new()
		{
			"unit.worker",
			"unit.scientist",
			"upgrade.commandcentre"
		};
	}
}
