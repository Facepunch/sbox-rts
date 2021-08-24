using Gamelib.FlowFields;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct MoveCommand : IMoveCommand
	{
		public List<Vector3> Positions { get; set; }

		public void Execute( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( agent is not UnitEntity unit )
				return;

			unit.SetMoveTarget( moveGroup );
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( agent is UnitEntity unit )
			{
				return unit.IsAtDestination();
			}

			return true;
		}
	}
}
