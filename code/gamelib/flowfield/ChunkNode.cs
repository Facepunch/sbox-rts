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
		public float Radius;

		private Chunk Chunk;
		private int X;
		private int Y;
		private int PathId;
		private int Distance;

		private ChunkNode[] Neighbours;
		private List<PortalNode> PortalNodes;

		public void Initialize( Vector3 worldPosition, int x, int y, Chunk chunk, float radius )
		{
			WorldPosition = worldPosition;
			Radius = radius;
			Chunk = chunk;
			X = x;
			Y = y;

			var entities = Physics.GetEntitiesInSphere( worldPosition, Radius );

			if ( entities.Count() > 0 )
			{
				foreach ( var e in entities )
				{
					Log.Info( e.ToString() + ": " + e.Position + " / " + worldPosition + " (Dist: " + e.Position.Distance( worldPosition ) + ")" );
					DebugOverlay.Sphere( worldPosition, Radius * 2f, Color.Cyan, true, 60f );
				}
			}

			IsWalkable = (entities.Count() == 0);

			if ( !IsWalkable )
			{
				Log.Info( "A tile is not walkable" );
			}
			else
			{
				DebugOverlay.Sphere( worldPosition, Radius * 2f, Color.Green, true, 60f );
			}

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

		public Vector3 GetDirection()
		{
			var direction = Vector3.Zero;
			var position = WorldPosition;
			var length = 0;

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
