namespace Gamelib.FlowFields
{
	public interface IMoveCommand
	{
		public Vector3 Position { get; set; }
		public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent );
		public void Execute( MoveGroup moveGroup );
	}
}
