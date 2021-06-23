using System;
using Gamelib.Math;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class FlowField
	{
		public static int CurrentFloodPathId = 0;
		public static int CurrentPathId = 0;

		public int WorldSize;
		public int ChunkSize;
		public int NodeSize;
		public int ChunksX;
		public int ChunksY;

		public List<Portal> m_Portals = new List<Portal>();

		public Chunk[,] Chunks;

		public void CreateWorld( int worldSize = 10000, int chunkSize = 100, int nodeSize = 10 )
		{
			CurrentFloodPathId = 0;

			WorldSize = worldSize;
			ChunkSize = chunkSize;
			NodeSize = nodeSize;
			ChunksX = worldSize / chunkSize;
			ChunksY = worldSize / chunkSize;

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
						List<Portal> portals = Chunk.GeneratePortals( this, chunk, horizontal, true );

						for ( int i = 0; i < portals.Count; ++i )
						{
							if ( !m_Portals.Contains( portals[i] ) )
							{
								m_Portals.Add( portals[i] );
							}
						}
					}

					if ( vertical != null )
					{
						var portals = Chunk.GeneratePortals( this, chunk, vertical, false );

						for ( int i = 0; i < portals.Count; ++i )
						{
							if ( !m_Portals.Contains( portals[i] ) )
							{
								m_Portals.Add( portals[i] );
							}
						}
					}
				}
			}
		}

		public void CreatePortalConnections()
		{
			for ( int i = 0; i < m_Portals.Count; ++i )
			{
				var portal = m_Portals[i];

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

		public Chunk GetChunkAtWorld( Vector3 position )
		{
			var center = GetOrigin();
			var distance = position - center;

			distance.x += WorldSize * 0.5f - ChunkSize * 0.5f;
			distance.y += WorldSize * 0.5f - ChunkSize * 0.5f;

			int x = (int)MathF.Round( distance.x / ChunkSize, 0f );
			int y = (int)MathF.Round( distance.y / ChunkSize, 0f );

			x = System.Math.Clamp( x, 0, ChunksX - 1 );
			y = System.Math.Clamp( y, 0, ChunksY - 1 );

			return Chunks[x, y];
		}

		public void FindPath( Vector3 position, Vector3 goal )
		{
			var astar = new AStar();
			astar.FindPath( ++CurrentPathId, this, position.WithZ( 0f ), goal.WithZ( 0f ) );
		}
	}
}
