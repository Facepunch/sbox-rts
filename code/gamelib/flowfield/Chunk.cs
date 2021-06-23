using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Chunk
	{
		public FlowField FlowField;
		public int NodeSize;
		public int NodesX;
		public int NodesY;
		public int X;
		public int Y;

		private ChunkNode[,] Nodes;

		public ChunkNode GetNode( int x, int y )
		{
			if ( x >= 0 && x < NodeSize )
			{
				if ( y >= 0 && y < NodeSize )
				{
					return Nodes[x, y];
				}
			}

			return null;
		}

		public void Initialize( int x, int y, FlowField world )
		{
			X = x;
			Y = y;
			FlowField = world;

			NodeSize = world.NodeSize;
			NodesX = world.ChunkSize / NodeSize;
			NodesY = world.ChunkSize / NodeSize;

			Nodes = new ChunkNode[NodeSize, NodeSize];

			for ( int px = 0; px < NodeSize; ++px )
			{
				for ( int py = 0; py < NodeSize; ++py )
				{
					Nodes[px, py] = new ChunkNode();
				}
			}

			for ( int px = 0; px < NodeSize; ++px )
			{
				for ( int py = 0; py < NodeSize; ++py )
				{
					Nodes[px, py].Initialize( px, py, this );
				}
			}
		}

		public void Flood( int floodId, ref ChunkNode[] chunkNodes, ref List<PortalNode> foundPortals )
		{
			var openList = new List<ChunkNode>();
			openList.AddRange( chunkNodes );

			for ( int i = 0; i < openList.Count; ++i )
			{
				var tile = openList[i];
				tile.SetPathId( floodId );
				tile.SetDistance( 0 );
			}

			for ( int i = 0; i < openList.Count; ++i )
			{
				var tile = openList[i];
				var neighbours = tile.GetNeighbours();

				for ( int j = 0; j < neighbours.Length; ++j )
				{
					var neighbour = neighbours[j];

					if ( neighbour.IsWalkable && !neighbour.HasPathId( floodId ) )
					{
						openList.Add( neighbour );
						neighbour.SetPathId( floodId );
						neighbour.SetDistance( tile.GetDistance() + 1 );
					}
				}

				if ( foundPortals != null )
				{
					var portals = tile.GetPortalNodes();

					if ( portals != null )
					{
						for ( int p = 0; p < portals.Count; ++p )
						{
							var portal = portals[p];

							if ( !foundPortals.Contains( portal ) )
							{
								foundPortals.Add( portal );
							}
						}
					}
				}
			}
		}

		public Vector3 GetWorldPosition()
		{
			var flowField = FlowField;
			var worldPosition = flowField.GetOrigin();
			var normalX = ((X / (float)flowField.ChunksX) * 2.0f) - 1.0f;
			var normalY = ((Y / (float)flowField.ChunksY) * 2.0f) - 1.0f;

			var centerX = normalX * 0.5f * flowField.WorldSize;
			var centerY = normalY * 0.5f * flowField.WorldSize;

			centerX += flowField.ChunkSize * 0.5f;
			centerY += flowField.ChunkSize * 0.5f;

			var relative = new Vector3( centerX, centerY );
			return worldPosition + relative;
		}

		public static List<Portal> GeneratePortals( FlowField world, Chunk chunkA, Chunk chunkB, bool isHorizontal )
		{
			var portals = new List<Portal>();
			var inPortal = false;
			var nodes = new List<ChunkNode>();
			var nodesB = new List<ChunkNode>();

			if ( !isHorizontal )
			{
				var bottomY = chunkA.NodeSize - 1;

				for ( int x = 0; x < chunkA.NodeSize; ++x )
				{
					var botA = chunkA.Nodes[x, bottomY];
					var topB = chunkB.Nodes[x, 0];
					var createPortal = TraversePortalEdge( botA, topB, ref inPortal );

					if ( inPortal )
					{
						nodes.Add( botA );
						nodesB.Add( topB );

						var isLastNode = (x == (chunkA.NodeSize - 1));

						if ( isLastNode )
						{
							createPortal = true;
						}
					}

					if ( createPortal )
					{
						var newPortal = new Portal();
						newPortal.Initialize( chunkA, nodes.ToArray(), chunkB, nodesB.ToArray(), world );
						portals.Add( newPortal );
						nodes.Clear();
						nodesB.Clear();

					}
				}

			}
			else
			{
				var rightX = chunkA.NodeSize - 1;

				for ( int y = 0; y < chunkA.NodeSize; ++y )
				{
					var bottomA = chunkA.Nodes[rightX, y];
					var topB = chunkB.Nodes[0, y];
					var createPortal = TraversePortalEdge( bottomA, topB, ref inPortal );

					if ( inPortal )
					{
						nodes.Add( bottomA );
						nodesB.Add( topB );

						var isLastNode = (y == (chunkA.NodeSize - 1));

						if ( isLastNode )
						{
							createPortal = true;
						}
					}

					if ( createPortal )
					{
						var newPortal = new Portal();
						newPortal.Initialize( chunkA, nodes.ToArray(), chunkB, nodesB.ToArray(), world );
						portals.Add( newPortal );
						nodes.Clear();
						nodesB.Clear();
					}
				}
			}

			return portals;
		}

		private static bool TraversePortalEdge( ChunkNode botA, ChunkNode topB, ref bool inPortal )
		{
			if ( !inPortal )
			{
				if ( botA.IsWalkable && topB.IsWalkable )
				{
					inPortal = true;
				}
			}

			if ( inPortal )
			{
				if ( !botA.IsWalkable || !topB.IsWalkable )
				{
					inPortal = false;
					return true;
				}
			}

			return false;
		}

		public ChunkNode GetNodeAtWorld( Vector3 position )
		{
			var center = GetWorldPosition();
			var distance = position - center;
			var flowField = FlowField;
			var chunkSize = flowField.ChunkSize;

			distance.x += chunkSize * 0.5f - NodeSize * 0.5f;
			distance.y += chunkSize * 0.5f - NodeSize * 0.5f;

			int x = (int)MathF.Round( (distance.x / chunkSize) * NodesX, 0f );
			int y = (int)MathF.Round( (distance.y / chunkSize) * NodesY, 0f );

			x = System.Math.Clamp( x, 0, NodesX - 1 );
			y = System.Math.Clamp( y, 0, NodesY - 1 );

			return Nodes[x, y];
		}
	}
}
