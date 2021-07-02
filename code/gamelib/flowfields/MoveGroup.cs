using Facepunch.RTS;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Maths;
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

		public MoveGroup()
		{
			Event.Register( this );

			ReachedGoal = new();
		}

		public void Initialize( List<IMoveAgent> agents, Vector3 destination, bool addVariety = false )
		{
			if ( addVariety && agents.Count > 1 )
			{
				Pathfinder = GetPathfinder( agents );

				var targetRadius = Pathfinder.Scale * agents.Count * 0.5f;
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

			for ( int i = 0; i < Agents.Count; i++ )
			{
				var other = Agents[i];

				if ( other.MoveGroup == this && ReachedGoal.Contains( other ) )
				{
					var distance = agent.Position.Distance( other.Position );

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
				Log.Error( "[MoveGroup] SortByDistance was called but the MoveGroup is invalid!" );
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
			pathfinders.Sort( ( a, b ) => a.Scale.CompareTo( b.Scale ) );
			return pathfinders[0];
		}

		private Pathfinder GetPathfinder( IMoveAgent agent )
		{
			return agent.Pathfinder;
		}
	}
}
