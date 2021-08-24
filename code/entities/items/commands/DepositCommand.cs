using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS.Commands
{
    public struct DepositCommand : IMoveCommand
	{
		public BuildingEntity Target { get; set; }
		public Vector3 Position { get; set; }

		public void Execute( MoveGroup moveGroup )
		{
			if ( !Target.IsValid() ) return;

			for ( int i = 0; i < moveGroup.Agents.Count; i++ )
			{
				var agent = moveGroup.Agents[i];

				if ( agent is not UnitEntity unit )
					continue;

				if ( unit.IsUsingAbility() )
					continue;

				unit.Deposit( Target, moveGroup );
			}
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
