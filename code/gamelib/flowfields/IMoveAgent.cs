namespace Gamelib.FlowFields
{
	public interface IMoveAgent
	{
		public Vector3 Position { get; }
		public Vector3 Velocity { get; }
		public float AgentRadius { get; }
		public Pathfinder Pathfinder { get; }
		public MoveGroup MoveGroup { get; }
		public void OnMoveGroupDisposed();
	}
}
