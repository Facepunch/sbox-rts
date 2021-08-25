using Gamelib.FlowFields;
using Gamelib.FlowFields.Extensions;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct GatherCommand : IMoveCommand
	{
		public ResourceEntity Target { get; set; }

		public void Execute( MoveGroup group, IMoveAgent agent )
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

		public List<Vector3> GetDestinations( MoveGroup group )
		{
			if ( !Target.IsValid() )
				return null;

			return Target.GetDestinations( group.Pathfinder, true );
		}

		public bool IsFinished( MoveGroup group, IMoveAgent agent )
		{
			return !Target.IsValid();
		}
	}
}
