using Sandbox;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gamelib.FlowField
{
	public class ChunkNode
	{
		public Vector3 WorldPosition;
		public bool IsWalkable = true;
		public float Diameter;

		private int PathId;
		private int Distance;

		private ChunkNode[] Neighbours;
		private List<PortalNode> PortalNodes;

		public void Initialize( Vector3 worldPosition, int x, int y, Chunk chunk, float radius )
		{
			WorldPosition = worldPosition;
			Diameter = radius;

			var entities = Physics.GetEntitiesInSphere( worldPosition, Diameter );

			IsWalkable = (entities.Count() == 0);

			var nodes = new List<ChunkNode>();
			var up = chunk.GetNode( x, y + 1 );
			var down = chunk.GetNode( x, y - 1 );
			var left = chunk.GetNode( x - 1, y );
			var right = chunk.GetNode( x + 1, y );

			if ( up != null )
			{
				nodes.Add( up );
			}

			if ( down != null )
			{
				nodes.Add( down );
			}

			if ( left != null )
			{
				nodes.Add( left );
			}

			if ( right != null )
			{
				nodes.Add( right );
			}

			Neighbours = nodes.ToArray();
			PathId = -1;

			if ( !IsWalkable )
			{
				Debug( Color.Red, 60f );
			}
		}

		public void SetPathId( int id )
		{
			PathId = id;
		}

		public bool HasPathId( int id )
		{
			return PathId == id;
		}

		public void SetDistance( int distance )
		{
			Distance = distance;
		}

		public int GetDistance()
		{
			return Distance;
		}

		public ChunkNode[] GetNeighbours()
		{
			return Neighbours;
		}

		public List<PortalNode> GetPortalNodes()
		{
			return PortalNodes;
		}

		public void SetPortalNode( PortalNode portalNode )
		{
			if ( PortalNodes == null )
			{
				PortalNodes = new List<PortalNode>();
			}

			if ( !PortalNodes.Contains( portalNode ) )
			{
				PortalNodes.Add( portalNode );
			}
		}

		public void Debug( Color color, float time = 0f )
		{
			var diameter = Diameter;
			var radius = diameter * 0.5f;
			DebugOverlay.Box( time, WorldPosition, new Vector3( -radius, -radius, 0f ), new Vector3( radius, radius, diameter ), color );
			DebugOverlay.Line( WorldPosition.WithZ( diameter ), WorldPosition.WithZ( diameter ) + GetDirection() * radius, Color.Blue, time );
			DebugOverlay.Circle( WorldPosition.WithZ( diameter ), Rotation.LookAt( Vector3.Up ), (diameter * 0.05f), Color.White, true, time );
		}

		public Vector3 GetDirection()
		{
			var direction = Vector3.Zero;
			var position = WorldPosition;
			int length = 0;

			for ( int i = 0; i < Neighbours.Length; ++i )
			{
				if ( Neighbours[i].IsWalkable )
				{
					length++;

					if ( Neighbours[i].Distance > Distance )
					{
						direction += position - Neighbours[i].WorldPosition;
					}
					else
					{
						direction -= position - Neighbours[i].WorldPosition;
					}
				}
			}

			direction /= length;

			return direction.Normal;
		}
	}
}
