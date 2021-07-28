using Facepunch.RTS;
using Gamelib.FlowFields;
using Gamelib.Maths;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamelib.FlowFields
{
	public class MoveGroup : IDisposable
	{
		public HashSet<IMoveAgent> ReachedGoal { get; private set; }
		public List<IMoveAgent> Agents { get; private set; }
		public PathRequest PathRequest { get; private set; }
		public Pathfinder Pathfinder { get; private set; }

		public MoveGroup( bool autoSortAgents = false )
		{
			if ( autoSortAgents )
			{
				Event.Register( this );
			}

			ReachedGoal = new();
		}

		public void Initialize( List<IMoveAgent> agents, Vector3 destination, bool addNearbyNodes = false )
		{
			if ( addNearbyNodes && agents.Count > 1 )
			{
				Pathfinder = GetPathfinder( agents );

				var targetRadius = Pathfinder.NodeSize * agents.Count * 0.5f;
				var destinations = new List<Vector3>();

				Pathfinder.GetGridPositions( destination, targetRadius, destinations );

				Initialize( agents, destinations );
			}
			else
			{
				Pathfinder = GetPathfinder( agents );
				PathRequest = Pathfinder.Request( destination );
				Agents = agents;
			}
		}

		public void Initialize( IMoveAgent agent, Vector3 destination )
		{
			Pathfinder = GetPathfinder( agent );
			PathRequest = Pathfinder.Request( destination );
			Agents = new List<IMoveAgent>() { agent };
		}

		public void Initialize( List<IMoveAgent> agents, List<Vector3> destinations )
		{
			if ( destinations.Count > 0 )
			{
				Pathfinder = GetPathfinder( agents );
				PathRequest = Pathfinder.Request( destinations );
				Agents = agents;
			}
		}

		public void Initialize( IMoveAgent agent, List<Vector3> destinations )
		{
			if ( destinations.Count > 0 )
			{
				Pathfinder = GetPathfinder( agent );
				PathRequest = Pathfinder.Request( destinations );
				Agents = new List<IMoveAgent>() { agent };
			}
		}

		public void Finish( IMoveAgent agent )
		{
			if ( !IsValid() || ReachedGoal.Contains( agent) )
				return;

			ReachedGoal.Add( agent );

			if ( ReachedGoal.Count == Agents.Count )
			{
				Dispose();
			}
		}

		public void Remove( IMoveAgent agent )
		{
			if ( !IsValid() ) return;

			ReachedGoal.Remove( agent );
			Agents.Remove( agent );

			if ( Agents.Count == 0 )
			{
				Dispose();
			}
		}

		public Vector3 GetDestination()
		{
			if ( !IsValid() ) return Vector3.Zero;

			return PathRequest.GetDestination();
		}

		public Vector3 GetDirection( Vector3 position )
		{
			if ( IsValid() )
				return PathRequest.GetDirection( position );

			return Vector3.Zero;
		}

		public bool IsDestination( IMoveAgent agent, Vector3 position )
		{
			if ( !IsValid() || ReachedGoal.Contains( agent ) )
				return true;

			if ( PathRequest.IsDestination( position ) )
				return true;

			var groundPosition = agent.Position.WithZ( 0f );

			for ( int i = 0; i < Agents.Count; i++ )
			{
				var other = Agents[i];

				if ( other.MoveGroup == this && ReachedGoal.Contains( other ) )
				{
					var distance = groundPosition.Distance( other.Position.WithZ( 0f ) );

					if ( distance <= agent.AgentRadius )
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
				Pathfinder.Complete( PathRequest );
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

			Event.Unregister( this );
		}

		public void ScaleSpeed( IMoveAgent agent, ref float speed )
		{
			var index = Agents.IndexOf( agent );

			for ( var i = index + 1; i < Agents.Count; i++ )
			{
				var other = Agents[i];
				var distance = other.Position.Distance( agent.Position );

				if ( distance <= agent.AgentRadius )
				{
					speed *= 0.6f;
				}
			}
		}

		[Event.Tick.Server]
		private void SortByDistance()
		{
			if ( !IsValid() )
			{
				Event.Unregister( this );
				return;
			}

			var destination = PathRequest.GetDestination();

			Agents.Sort( ( a, b ) =>
			{
				return b.Position.Distance( destination ).CompareTo( a.Position.Distance( destination ) );
			} );
		}

		private Pathfinder GetPathfinder( List<IMoveAgent> agents )
		{
			var pathfinders = agents.Select( a => a.Pathfinder ).ToList();
			pathfinders.Sort( ( a, b ) => a.CollisionSize.CompareTo( b.CollisionSize ) );
			return pathfinders[0];
		}

		private Pathfinder GetPathfinder( IMoveAgent agent )
		{
			return agent.Pathfinder;
		}
	}
}
