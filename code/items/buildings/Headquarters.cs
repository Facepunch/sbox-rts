﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
	[Library]
	public class Headquarters : BaseBuilding
	{
		public override string Name => "Headquarters";
		public override string UniqueId => "building.headquarters";
		public override Texture Icon => Texture.Load( "textures/rts/tempicons/headquarters.png" );
		public override string Description => "This is the heart of your empire. Protect it at all costs.";
		public override bool CanDepositResources => true;
		public override float MaxHealth => 2000f;
		public override float MinLineOfSight => 500f;
		public override int BuildTime => 60;
		public override bool CanDemolish => false;
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
			"unit.scientist"
		};
	}
}
