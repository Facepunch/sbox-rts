using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Headquarters : BaseBuilding
	{
		public override string Name => "Headquarters";
		public override string UniqueId => "building.headquarters";
		public override Texture Icon => Texture.Load( "textures/rts/icons/headquarters.png" );
		public override string Description => "This is the heart of your empire. Protect it at all costs.";
		public override bool CanDepositResources => true;
		public override float MinLineOfSight => 500f;
		public override int BuildTime => 10;
		public override HashSet<string> Abilities => new()
		{
			"ability_airstrike"
		};
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Stone] = 1000,
			[ResourceType.Metal] = 500
		};
		public override string Model => "models/buildings/headquarters/headquarters.vmdl";
		public override HashSet<string> Queueables => new()
		{
			"unit.worker",
			"unit.scientist",
			"tech.machinery"
		};
	}
}
