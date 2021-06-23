using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Chunk
	{
		public FlowField FlowField;
		public ChunkNode[,] Nodes;
		public float NodeRadius;
		public int Size;
		public int X;
		public int Y;

		public ChunkNode GetNode( int x, int y )
		{
			if ( x >= 0 && x < Size )
			{
				if ( y >= 0 && y < Size )
				{
					return Nodes[x, y];
				}
			}

			return null;
		}

		public void Initialize( int x, int y, FlowField world )
		{
			NodeRadius = world.NodeRadius;
			FlowField = world;
			Size = (int)MathF.Floor( world.ChunkSize / NodeRadius );
			Nodes = new ChunkNode[Size, Size];
			X = x;
			Y = y;

			for ( int px = 0; px < Size; ++px )
			{
				for ( int py = 0; py < Size; ++py )
				{
					Nodes[px, py] = new ChunkNode();
				}
			}

			var worldTopLeft = world.WorldTopLeft;
			var startGridX = (x * world.ChunkSize);
			var startGridY = (y * world.ChunkSize);
			var nodeRadius = world.NodeRadius;

			for ( int px = 0; px < Size; ++px )
			{
				for ( int py = 0; py < Size; ++py )
				{
					var worldGridX = startGridX + (px * nodeRadius);
					var worldGridY = startGridY + (py * nodeRadius);
					var worldPosition = worldTopLeft + Vector3.Forward * worldGridX;
					worldPosition += Vector3.Left * worldGridY;

					Nodes[px, py].Initialize( worldPosition, px, py, this, NodeRadius );
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
				var chunk = openList[i];
				var neighbours = chunk.GetNeighbours();

				for ( int j = 0; j < neighbours.Length; ++j )
				{
					var neighbour = neighbours[j];

					if ( neighbour.IsWalkable && !neighbour.HasPathId( floodId ) )
					{
						openList.Add( neighbour );
						neighbour.SetPathId( floodId );
						neighbour.SetDistance( chunk.GetDistance() + 1 );
					}
				}

				if ( foundPortals != null )
				{
					var portals = chunk.GetPortalNodes();

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

		public static List<Portal> GeneratePortals( FlowField world, Chunk chunkA, Chunk chunkB, bool isHorizontal )
		{
			var portals = new List<Portal>();
			var inPortal = false;
			var nodesA = new List<ChunkNode>();
			var nodesB = new List<ChunkNode>();

			if ( !isHorizontal )
			{
				var bottomY = chunkA.Size - 1;

				for ( int x = 0; x < chunkA.Size; ++x )
				{
					var botA = chunkA.Nodes[x, bottomY];
					var topB = chunkB.Nodes[x, 0];
					var createPortal = TraversePortalEdge( botA, topB, ref inPortal );

					if ( inPortal )
					{
						nodesA.Add( botA );
						nodesB.Add( topB );

						var isLastNode = (x == (chunkA.Size - 1));

						if ( isLastNode )
						{
							createPortal = true;
						}
					}

					if ( createPortal )
					{
						var newPortal = new Portal();
						newPortal.Initialize( chunkA, nodesA.ToArray(), chunkB, nodesB.ToArray(), world );
						portals.Add( newPortal );
						nodesA.Clear();
						nodesB.Clear();

					}
				}
			}
			else
			{
				var rightX = chunkA.Size - 1;

				for ( int y = 0; y < chunkA.Size; ++y )
				{
					var bottomA = chunkA.Nodes[rightX, y];
					var topB = chunkB.Nodes[0, y];
					var createPortal = TraversePortalEdge( bottomA, topB, ref inPortal );

					if ( inPortal )
					{
						nodesA.Add( bottomA );
						nodesB.Add( topB );

						var isLastNode = (y == (chunkA.Size - 1));

						if ( isLastNode )
						{
							createPortal = true;
						}
					}

					if ( createPortal )
					{
						var newPortal = new Portal();
						newPortal.Initialize( chunkA, nodesA.ToArray(), chunkB, nodesB.ToArray(), world );
						portals.Add( newPortal );
						nodesA.Clear();
						nodesB.Clear();
					}
				}
			}

			return portals;
		}

		private static bool TraversePortalEdge( ChunkNode bottomA, ChunkNode topB, ref bool inPortal )
		{
			if ( !inPortal )
			{
				if ( bottomA.IsWalkable && topB.IsWalkable )
				{
					inPortal = true;
				}
			}

			if ( inPortal )
			{
				if ( !bottomA.IsWalkable || !topB.IsWalkable )
				{
					inPortal = false;
					return true;
				}
			}

			return false;
		}
	}
}
