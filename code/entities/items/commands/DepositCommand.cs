using Gamelib.FlowFields;
using Gamelib.FlowFields.Extensions;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct DepositCommand : IMoveCommand
	{
		public BuildingEntity Target { get; set; }

		public void Execute( MoveGroup group, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return;

			if ( agent is not UnitEntity unit )
				return;

			if ( unit.IsUsingAbility() )
				return;

			unit.SetDepositTarget( Target );
		}

		public List<Vector3> GetDestinations( MoveGroup group )
		{
			if ( !Target.IsValid() )
				return null;

			return Target.GetDestinations( group.Pathfinder, true );
		}

		public bool IsFinished( MoveGroup group, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return true;

			if ( agent is UnitEntity unit )
			{
				return unit.Carrying.Count == 0;
			}

			return true;
		}
	}
}
