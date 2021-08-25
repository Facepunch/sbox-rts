using Gamelib.FlowFields;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct MoveCommand : IMoveCommand
	{
		public List<Vector3> Destinations { get; set; }

		public MoveCommand( Vector3 destination )
		{
			Destinations = new List<Vector3>() { destination };
		}

		public MoveCommand( List<Vector3> destinations )
		{
			Destinations = destinations;
		}

		public void Execute( MoveGroup group, IMoveAgent agent )
		{
			if ( agent is not UnitEntity unit )
				return;

			unit.SetMoveTarget( group );
		}

		public List<Vector3> GetDestinations( MoveGroup group )
		{
			return Destinations; ;
		}

		public bool IsFinished( MoveGroup group, IMoveAgent agent )
		{
			if ( agent is UnitEntity unit )
			{
				return unit.IsAtDestination();
			}

			return true;
		}
	}
}
