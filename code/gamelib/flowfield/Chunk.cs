using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Chunk
	{
		public FlowField FlowField;
		public ChunkNode[,] Nodes;
		public float NodeDiameter;
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
			NodeDiameter = world.NodeDiameter;
			FlowField = world;
			Size = (int)MathF.Ceiling( world.ChunkSize / NodeDiameter );
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
			var nodeDiameter = world.NodeDiameter;
			var startGridX = (x * world.ChunkSize);
			var startGridY = (y * world.ChunkSize);

			for ( int px = 0; px < Size; ++px )
			{
				for ( int py = 0; py < Size; ++py )
				{
					var worldGridX = startGridX + (px * nodeDiameter);
					var worldGridY = startGridY + (py * nodeDiameter);
					var worldPosition = worldTopLeft + Vector3.Forward * worldGridX;
					worldPosition += Vector3.Left * worldGridY;

					Nodes[px, py].Initialize( worldPosition, px, py, this, nodeDiameter );
				}
			}
		}

		public void Flood( int floodId, ref ChunkNode[] chunkNodes, ref List<PortalNode> foundPortals )
		{
			var openList = new List<ChunkNode>();
			openList.AddRange( chunkNodes );

			var worldTopLeft = FlowField.WorldTopLeft;
			var startGridX = (X * FlowField.ChunkSize);
			var startGridY = (Y * FlowField.ChunkSize);
			var worldPosition = worldTopLeft + Vector3.Forward * startGridX;
			worldPosition += Vector3.Left * startGridY;

			DebugOverlay.Box( 5f, worldPosition.WithZ( 80f ), new Vector3( 0f, 0f, 0f ), new Vector3( FlowField.ChunkSize, FlowField.ChunkSize, 500f ), Color.Cyan );

			for ( int i = 0; i < openList.Count; ++i )
			{
				var node = openList[i];
				node.SetPathId( floodId );
				node.SetDistance( 0 );
				//node.Debug( Color.White, 5f );
			}
			
			Log.Info( "Resetting chunk " + X + ", " + Y + " node distances to 0: " + openList.Count + " nodes" );

			for ( int i = 0; i < openList.Count; ++i )
			{
				var node = openList[i];
				var neighbours = node.GetNeighbours();

				for ( int j = 0; j < neighbours.Length; ++j )
				{
					var neighbour = neighbours[j];

					if ( neighbour.IsWalkable && !neighbour.HasPathId( floodId ) )
					{
						openList.Add( neighbour );
						neighbour.SetPathId( floodId );
						neighbour.SetDistance( node.GetDistance() + 1 );
					}
				}

				if ( foundPortals != null )
				{
					var portals = node.GetPortalNodes();

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
					var bottomA = chunkA.Nodes[x, bottomY];
					var topB = chunkB.Nodes[x, 0];
					var createPortal = TraversePortalEdge( bottomA, topB, ref inPortal );

					if ( inPortal )
					{
						nodesA.Add( bottomA );
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
