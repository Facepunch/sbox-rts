using Gamelib.FlowFields;

namespace Facepunch.RTS.Commands
{
    public struct MoveCommand : IMoveCommand
	{
		public Vector3 Position { get; set; }

		public void Execute( MoveGroup moveGroup )
		{
			for ( int i = 0; i < moveGroup.Agents.Count; i++ )
			{
				var agent = moveGroup.Agents[i];

				if ( agent is not UnitEntity unit )
					return;

				unit.MoveTo( moveGroup );
			}
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
