﻿using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct ConstructCommand : IMoveCommand
	{
		public BuildingEntity Target { get; set; }
		public List<Vector3> Positions { get; set; }

		public void Execute( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() || !Target.IsUnderConstruction )
				return;

			if ( agent is not UnitEntity unit )
				return;

			if ( unit.IsUsingAbility() )
				return;

			if ( !unit.CanConstruct )
				return;

			unit.SetConstructTarget( Target );
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return true;
			return !Target.IsUnderConstruction;
		}
	}
}
