using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class ChunkNode
	{
		public bool IsWalkable = true;

		private Chunk Chunk;
		private int X;
		private int Y;
		private int PathId;
		private int Distance;

		private ChunkNode[] Neighbours;
		private List<PortalNode> PortalNodes;

		public void Initialize( int x, int y, Chunk chunk )
		{
			X = x;
			Y = y;
			Chunk = chunk;

			/*
			Vector3 worldPos = GetWorldPosition();
			RaycastHit hitInfo;

			float rad = Mathf.Max(m_Tile.m_TileNodeXSize, m_Tile.m_TileNodeYSize);
			if( Physics.SphereCast(worldPos, rad, Vector3.forward, out hitInfo, 100.0f) == true)
			{
				IsWalkable = false;
			}
			*/

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

		public Vector3 GetWorldPosition()
		{
			var chunkWorldPosition = Chunk.GetWorldPosition();
			var flowField = Chunk.FlowField;

			var normalX = ((X / (float)flowField.ChunksX) * 2.0f) - 1.0f;
			var normalY = ((Y / (float)flowField.ChunksY) * 2.0f) - 1.0f;

			var centerX = normalX * 0.5f * flowField.ChunkSize;
			var centerY = normalY * 0.5f * flowField.ChunkSize;

			centerX += Chunk.NodeSize * 0.5f;
			centerY += Chunk.NodeSize * 0.5f;

			var relative = new Vector3( centerX, centerY );

			return chunkWorldPosition + relative;
		}

		public Vector3 GetDirection()
		{
			var direction = Vector3.Zero;
			var position = GetWorldPosition();

			for ( int i = 0; i < Neighbours.Length; ++i )
			{
				if ( Neighbours[i].IsWalkable )
				{
					if ( Neighbours[i].Distance > Distance )
					{
						direction += Neighbours[i].GetWorldPosition() - position;
					}
					else
					{
						direction -= Neighbours[i].GetWorldPosition() - position;
					}
				}
			}

			return direction.Normal * -1f;
		}
	}
}
