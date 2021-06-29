using Facepunch.RTS;
using Gamelib.FlowFields;
using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
	public class MoveGroup
	{
		public HashSet<IFlockAgent> ReachedGoal { get; private set; }
		public List<IFlockAgent> Agents { get; private set; }
		public PathRequest PathRequest { get; private set; }

		public MoveGroup( List<IFlockAgent> agents, Vector3 destination )
		{
			ReachedGoal = new();
			PathRequest = PathManager.Instance.Request( destination );
			Agents = agents;
		}

		public MoveGroup( IFlockAgent agent, Vector3 destination )
		{
			ReachedGoal = new();
			PathRequest = PathManager.Instance.Request( destination );
			Agents = new List<IFlockAgent>() { agent };
		}

		public MoveGroup( List<IFlockAgent> agents, List<Vector3> destinations )
		{
			if ( destinations.Count > 0 )
			{
				ReachedGoal = new();
				PathRequest = PathManager.Instance.Request( destinations );
				Agents = agents;
			}
		}

		public MoveGroup( IFlockAgent agent, List<Vector3> destinations )
		{
			if ( destinations.Count > 0 )
			{
				ReachedGoal = new();
				PathRequest = PathManager.Instance.Request( destinations );
				Agents = new List<IFlockAgent>() { agent };
			}
		}

		public void Finish( UnitEntity unit )
		{
			if ( !IsValid() ) return;

			ReachedGoal.Add( unit );

			if ( ReachedGoal.Count == Agents.Count )
			{
				Dispose();
			}
		}

		public void Remove( UnitEntity unit )
		{
			if ( !IsValid() ) return;

			ReachedGoal.Remove( unit );
			Agents.Remove( unit );

			if ( Agents.Count == 0 )
			{
				Dispose();
			}
		}

		public Vector3 GetDirection( Vector3 position )
		{
			if ( IsValid() ) return PathRequest.GetDirection( position );
			return Vector3.Zero;
		}

		public bool IsDestination( IFlockAgent agent, Vector3 position )
		{
			if ( !IsValid() || ReachedGoal.Contains( agent ) )
				return true;

			if ( PathRequest.IsDestination( position ) )
				return true;

			for ( int i = 0; i < Agents.Count; i++ )
			{
				var other = Agents[i];

				if ( other.MoveGroup == this && ReachedGoal.Contains( other ) )
				{
					var distance = agent.Position.Distance( other.Position );

					if ( distance <= (agent.FlockSettings.Radius * 2f) )
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool IsValid()
		{
			return (PathRequest != null && Agents != null);
		}

		public void Dispose()
		{
			if ( PathRequest != null && PathRequest.IsValid() )
			{
				RTS.Path.Complete( PathRequest );

				PathRequest = null;

				for ( int i = 0; i < Agents.Count; i++ )
				{
					var agent = Agents[i];

					if ( agent.MoveGroup == this )
					{
						agent.OnMoveGroupDisposed();
					}
				}

				Agents = null;
			}
		}
	}
}
