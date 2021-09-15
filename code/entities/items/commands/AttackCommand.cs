using Gamelib.FlowFields;
using Gamelib.FlowFields.Extensions;
using System.Collections.Generic;
using Sandbox;

namespace Facepunch.RTS.Commands
{
    public struct AttackCommand : IMoveCommand
	{
		public IDamageable Target { get; set; }

		public void Execute( MoveGroup group, IMoveAgent agent )
		{
			var entity = (ModelEntity)Target;

			if ( !entity.IsValid() )
				return;

			if ( agent is not UnitEntity unit )
				return;

			if ( !unit.IsUsingAbility() && unit.CanAttackTarget( Target ) )
			{
				unit.SetAttackTarget( Target, true );
			}
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
			return !entity.IsValid();
		}
	}
}
