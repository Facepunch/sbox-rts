using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct RepairCommand : IMoveCommand
	{
		public BuildingEntity Target { get; set; }
		public List<Vector3> Positions { get; set; }

		public void Execute( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return;

			if ( agent is not UnitEntity unit )
				return;

			if ( unit.IsUsingAbility() )
				return;

			unit.SetRepairTarget( Target );
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() || Target.Health == Target.MaxHealth )
				return true;

			return false;
		}
	}
}
