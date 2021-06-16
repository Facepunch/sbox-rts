﻿using Sandbox;
using System.Collections.Generic;

namespace RTS.Units
{
	[Library]
	public class Scout : BaseUnit
	{
		public override string Name => "Scout";
		public override string UniqueId => "unit.scout";
		public override string Model => "models/vehicles/humvee/humvee.vmdl";
		public override string Description => "A basic vehicle good for scouting large areas.";
		public override bool UseRenderColor => true;
		public override float LineOfSight => 1000f;
		public override float Speed => 500f;
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 100
		};
	}
}