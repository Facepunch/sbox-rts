namespace Gamelib.FlowFields
{
	public struct FlockSettings
	{
		public float Radius;
		public float MaxSpeed;
		public float MaxForce;
	}

	public interface IFlockAgent
	{
		public Vector3 Position { get; }
		public Vector3 Velocity { get; }
		public FlockSettings FlockSettings { get; }
		public Pathfinder Pathfinder { get; }
		public MoveGroup MoveGroup { get; }
		public void OnMoveGroupDisposed();
	}
}
