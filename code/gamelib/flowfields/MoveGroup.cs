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
		private struct SimpleMoveCommand : IMoveCommand
		{
			public Vector3 Position { get; set; }

			public void Execute( MoveGroup moveGroup ) { }

			public bool IsFinished( MoveGroup moveGroup, IMoveAgent agent )
			{
				return moveGroup.IsDestination( agent, agent.Position );
			}
		}

		public HashSet<IMoveAgent> ReachedGoal { get; private set; }
		public List<IMoveAgent> Agents { get; private set; }
		public PathRequest PathRequest { get; private set; }
		public Pathfinder Pathfinder { get; private set; }
		public Queue<IMoveCommand> Queue { get; set; }

		private IMoveCommand Command { get; set; }

		public MoveGroup()
		{
			ReachedGoal = new();
			Queue = new();
		}

		public void Initialize( IMoveAgent agent, IMoveCommand command )
		{
			Pathfinder = GetPathfinder( agent );
			Agents = new List<IMoveAgent>() { agent };
			Queue.Enqueue( command );
			NextCommand();
		}

		public void Initialize( List<IMoveAgent> agents, IMoveCommand command )
		{
			Pathfinder = GetPathfinder( agents );
			Agents = agents;
			Queue.Enqueue( command );
			NextCommand();
		}

		public void Enqueue( MoveGroup other )
		{
			if ( other == null ) return;

			foreach ( var command in other.Queue )
			{
				Enqueue( command );
			}
		}

		public void Enqueue( IMoveCommand command )
		{
			Queue.Enqueue( command );
		}

		public void Enqueue( Vector3 destination )
		{
			Queue.Enqueue( new SimpleMoveCommand
			{
				Position = destination
			} );
		}

		public bool Finish( IMoveAgent agent )
		{
			if ( !IsValid() || ReachedGoal.Contains( agent) )
				return true;

			if ( !Command.IsFinished( this, agent ) )
				return false;

			ReachedGoal.Add( agent );

			if ( ReachedGoal.Count == Agents.Count )
			{
				NextCommand();
			}

			return true;
		}

		public void NextCommand()
		{
			if ( Queue.Count == 0 || Agents.Count == 0 )
			{
				Dispose();
				return;
			}

			Command = Queue.Dequeue();

			PathRequest = Pathfinder.Request( Command.Position );
			ReachedGoal.Clear();

			Command.Execute( this );
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
			if ( !IsValid() || PathRequest == null )
				return Vector3.Zero;

			return PathRequest.GetDestination();
		}

		public Vector3 GetDirection( Vector3 position )
		{
			if ( IsValid() && PathRequest != null )
				return PathRequest.GetDirection( position );

			return Vector3.Zero;
		}

		public bool IsDestination( IMoveAgent agent, Vector3 position, bool checkPathRequest = true )
		{
			if ( !IsValid() || ReachedGoal.Contains( agent ) )
				return true;

			if ( checkPathRequest && ( PathRequest == null || PathRequest.IsDestination( position ) ) )
				return true;

			var groundPosition = agent.Position.WithZ( 0f );

			for ( int i = 0; i < Agents.Count; i++ )
			{
				var other = Agents[i];

				if ( other.MoveGroup == this && ReachedGoal.Contains( other ) )
				{
					var distance = groundPosition.Distance( other.Position.WithZ( 0f ) );

					if ( distance <= agent.AgentRadius )
						return true;
				}
			}

			return false;
		}

		public bool IsValid()
		{
			return Agents.Count > 0;
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

				Agents.Clear();
			}
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
