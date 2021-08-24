using System.Collections.Generic;

namespace Gamelib.FlowFields
{
	public interface IMoveCommand
	{
		public List<Vector3> Positions { get; set; }
		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent );
		public void Execute( MoveGroup moveGroup, IMoveAgent agent );
	}
}
