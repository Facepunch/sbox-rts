using Sandbox;

namespace Gamelib.Nav
{
	public class NavSteer
	{
		public NavSteerOutput Output;
		public ModelEntity Agent;
		public Vector3 Target;

		protected NavPath Path { get; private set; }

		public NavSteer( ModelEntity agent )
		{
			Agent = agent;
			Path = new();
		}

		public virtual void Tick( Vector3 currentPosition )
		{
			Path.Update( currentPosition, Target );

			Output.Finished = Path.IsEmpty;

			if ( Output.Finished )
			{
				Output.Direction = Vector3.Zero;
				return;
			}

			Output.Direction = Path.GetDirection( currentPosition );

			var avoid = GetAvoidance( currentPosition );

			if ( !avoid.IsNearlyZero() )
			{
				Output.Direction = (Output.Direction + avoid).Normal;
			}
		}

		private Vector3 GetAvoidance( Vector3 position )
		{
			return Vector3.Zero;
		}

		public struct NavSteerOutput
		{
			public bool Finished;
			public Vector3 Direction;
		}
	}
}
