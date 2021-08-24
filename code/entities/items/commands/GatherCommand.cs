using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS.Commands
{
    public struct GatherCommand : IMoveCommand
	{
		public ResourceEntity Target { get; set; }
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

				if ( !unit.CanGather( Target.Resource ) )
					continue;

				unit.Gather( Target, moveGroup );
			}
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			return !Target.IsValid();
		}
	}
}
