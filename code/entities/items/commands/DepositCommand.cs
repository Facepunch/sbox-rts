using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct DepositCommand : IMoveCommand
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

			unit.SetDepositTarget( Target );
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
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
