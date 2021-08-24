using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS.Commands
{
    public struct AttackCommand : IMoveCommand
	{
		public IDamageable Target { get; set; }
		public Vector3 Position { get; set; }

		public void Execute( MoveGroup moveGroup )
		{
			if ( !((Entity)Target).IsValid() ) return;

			for ( int i = 0; i < moveGroup.Agents.Count; i++ )
			{
				var agent = moveGroup.Agents[i];

				if ( agent is not UnitEntity unit ) return;

				if ( !unit.IsUsingAbility() && unit.InVerticalRange( (Entity)Target ) )
				{
					unit.Attack( Target, true, moveGroup );
				}
			}
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !((Entity)Target).IsValid() )
				return true;

			return false;
		}
	}
}
