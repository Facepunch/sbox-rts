using System;
using Gamelib.Math;
using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace Gamelib.FlowField
{
	public class FlowField
	{
		public static int CurrentFloodPathId = 0;
		public static int CurrentPathId = 0;

		public Vector3 WorldTopLeft;
		public int WorldSize;
		public float NodeRadius;
		public float NodeDiameter;
		public int ChunkSize;
		public int NodeSize;
		public int ChunksX;
		public int ChunksY;

		public List<Portal> Portals = new();
		public List<PortalNode> CurrentPortals = new();

		public Chunk[,] Chunks;

		public void CreateWorld( int worldSize, int chunkSize, float nodeRadius )
		{
			CurrentFloodPathId = 0;

			WorldSize = worldSize;
			ChunkSize = chunkSize;
			NodeRadius = nodeRadius;
			NodeDiameter = nodeRadius * 2f;
			ChunksX = worldSize / chunkSize;
			ChunksY = worldSize / chunkSize;
			WorldTopLeft = Vector3.Zero - Vector3.Forward * WorldSize / 2f - Vector3.Left * WorldSize / 2f;

			CreateChunks();
			CreatePortals();
			CreatePortalConnections();
		}

		public void CreateChunks()
		{
			Chunks = new Chunk[ChunksX, ChunksY];

			for ( int x = 0; x < ChunksX; ++x )
			{
				for ( int y = 0; y < ChunksY; ++y )
				{
					Chunks[x, y] = new Chunk();
					Chunks[x, y].Initialize( x, y, this );
				}
			}
		}

		public void CreatePortals()
		{
			for ( int x = 0; x < ChunksX; ++x )
			{
				for ( int y = 0; y < ChunksY; ++y )
				{
					Chunk chunk = Chunks[x, y];
					Chunk horizontal = null;
					Chunk vertical = null;

					if ( x + 1 < ChunksX )
					{
						horizontal = Chunks[x + 1, y];
					}

					if ( y + 1 < ChunksY )
					{
						vertical = Chunks[x, y + 1];
					}

					if ( horizontal != null )
					{
						var portals = Chunk.GeneratePortals( this, chunk, horizontal, true );

						for ( int i = 0; i < portals.Count; ++i )
						{
							if ( !Portals.Contains( portals[i] ) )
							{
								Portals.Add( portals[i] );
							}
						}
					}

					if ( vertical != null )
					{
						var portals = Chunk.GeneratePortals( this, chunk, vertical, false );

						for ( int i = 0; i < portals.Count; ++i )
						{
							if ( !Portals.Contains( portals[i] ) )
							{
								Portals.Add( portals[i] );
							}
						}
					}
				}
			}
		}

		public void CreatePortalConnections()
		{
			for ( int i = 0; i < Portals.Count; ++i )
			{
				var portal = Portals[i];

				List<PortalNode> distinctPortals = new();

				CurrentFloodPathId++;

				var foundPortals = portal.NodeA.FloodChunk( CurrentFloodPathId );
				foundPortals.Add( portal.NodeB );
				distinctPortals.Clear();

				for ( int k = 0; k < foundPortals.Count; ++k )
				{
					var p = foundPortals[k];

					if ( !distinctPortals.Contains( p ) )
					{
						distinctPortals.Add( p );
					}
				}

				portal.NodeA.Connections = distinctPortals.ToArray();

				CurrentFloodPathId++;

				foundPortals = portal.NodeB.FloodChunk( CurrentFloodPathId );
				foundPortals.Add( portal.NodeA );
				distinctPortals.Clear();

				for ( int k = 0; k < foundPortals.Count; ++k )
				{
					var p = foundPortals[k];

					if ( !distinctPortals.Contains( p ) )
					{
						distinctPortals.Add( p );
					}
				}

				portal.NodeB.Connections = distinctPortals.ToArray();
			}
		}

		public Vector3 GetOrigin()
		{
			return Vector3.Zero;
		}

		public ChunkNode GetNodeAtWorld( Vector3 position )
		{
			var localPosition = WorldToLocal( position );
			return GetNodeFromLocal( localPosition.x, localPosition.y );
		}

		public Chunk GetChunkAtWorld( Vector3 position )
		{
			var localPosition = WorldToLocal( position );
			return GetChunkFromLocal( localPosition.x, localPosition.y );
		}

		public Vector2i WorldToLocal( Vector2 position )
		{
			var worldSize = WorldSize;
			var px = ((position.x + worldSize / 2f) / worldSize);
			var py = ((position.y + worldSize / 2f) / worldSize);

			px = px.Clamp( 0f, 1f );
			py = py.Clamp( 0f, 1f );

			var fx = worldSize * px;
			var x = fx.FloorToInt().Clamp( 0, worldSize - 1 );

			var fy = worldSize * py;
			var y = fy.FloorToInt().Clamp( 0, worldSize - 1 );

			return new Vector2i( x, y );
		}

		public ChunkNode GetNodeFromLocal( int x, int y )
		{
			var chunk = GetChunkFromLocal( x, y );
			var nodeX = ((float)((x % ChunkSize) / chunk.NodeRadius)).FloorToInt();
			var nodeY = ((float)((y % ChunkSize) / chunk.NodeRadius)).FloorToInt();
			return chunk.Nodes[nodeX, nodeY];
		}

		public Chunk GetChunkFromLocal( int x, int y )
		{
			var chunkX = ((float)(x / ChunkSize)).CeilToInt();
			var chunkY = ((float)(y / ChunkSize)).CeilToInt();
			return Chunks[chunkX, chunkY];
		}

		public List<PortalNode> FindPath( Vector3 position, Vector3 goal )
		{
			var astar = new AStar();
			return astar.FindPath( ++CurrentPathId, this, position.WithZ( 0f ), goal.WithZ( 0f ) );
		}
	}
}
