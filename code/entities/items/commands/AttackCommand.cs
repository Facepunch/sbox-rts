using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Commands
{
    public struct AttackCommand : IMoveCommand
	{
		public IDamageable Target { get; set; }
		public List<Vector3> Positions { get; set; }

		public void Execute( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !IsTargetValid() )
				return;

			if ( agent is not UnitEntity unit )
				return;

			if ( !unit.IsUsingAbility() && unit.InVerticalRange( (Entity)Target ) )
			{
				unit.SetAttackTarget( Target, true );
			}
		}

		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
		{
			if ( !IsTargetValid() )
				return true;

			return false;
		}

		private bool IsTargetValid()
		{
			return ((Entity)Target).IsValid();
		}
	}
}
