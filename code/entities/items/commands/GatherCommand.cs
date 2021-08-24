using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct GatherCommand : IMoveCommand
	{
		public ResourceEntity Target { get; set; }
		public List<Vector3> Positions { get; set; }

		public void Execute( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return;

			if ( agent is not UnitEntity unit )
				return;

			if ( unit.IsUsingAbility() )
				return;

			if ( !unit.CanGather( Target.Resource ) )
				return;

			unit.SetGatherTarget( Target );
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			return !Target.IsValid();
		}
	}
}
