using Sandbox;

namespace Gamelib.Nav
{
	public class NavSteer
	{
		public NavSteerOutput Output;
		public Vector3 Target;

		protected NavPath Path { get; private set; }

		public NavSteer()
		{
			Path = new();
		}

		public virtual void Tick( Vector3 currentPosition )
		{
			Path.Update( currentPosition, Target );

			Output.Finished = Path.IsEmpty;

			if ( Output.Finished )
				return;

			Output.Direction = (Output.Direction + Path.GetDirection( currentPosition )).Normal;
		}

		public struct NavSteerOutput
		{
			public bool Finished;
			public Vector3 Direction;
		}
	}
}
