using Gamelib.FlowFields;
using Gamelib.FlowFields.Extensions;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct OccupyCommand : IMoveCommand
	{
		public IOccupiableEntity Target { get; set; }

		public void Execute( MoveGroup group, IMoveAgent agent )
		{
			var entity = (ModelEntity)Target;

			if ( !entity.IsValid() )
				return;

			if ( agent is not UnitEntity unit )
				return;

			if ( unit.IsUsingAbility() )
				return;

			unit.SetOccupyTarget( Target );
		}

		public List<Vector3> GetDestinations( MoveGroup group )
		{
			var entity = (ModelEntity)Target;

			if ( !entity.IsValid() )
				return null;

			return entity.GetDestinations( group.Pathfinder, true );
		}

		public bool IsFinished( MoveGroup group, IMoveAgent agent )
		{
			var entity = (ModelEntity)Target;

			if ( !entity.IsValid() )
				return true;

			if ( agent is UnitEntity unit )
			{
				return unit.Occupiable == Target;
			}

			return true;
		}
	}
}
