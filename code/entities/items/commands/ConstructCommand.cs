using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS.Commands
{
    public struct ConstructCommand : IMoveCommand
	{
		public BuildingEntity Target { get; set; }
		public Vector3 Position { get; set; }

		public void Execute( MoveGroup moveGroup )
		{
			if ( !Target.IsValid() || !Target.IsUnderConstruction )
				return;

			for ( int i = 0; i < moveGroup.Agents.Count; i++ )
			{
				var agent = moveGroup.Agents[i];

				if ( agent is not UnitEntity unit )
					continue;

				if ( unit.IsUsingAbility() )
					continue;

				if ( !unit.CanConstruct )
					continue;

				unit.Construct( Target, moveGroup );
			}
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !Target.IsValid() ) return true;
			return !Target.IsUnderConstruction;
		}
	}
}
