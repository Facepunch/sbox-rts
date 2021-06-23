using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class PortalNode
	{
		public Portal Portal;
		public Chunk Chunk;
		public ChunkNode[] ChunkNodes;
		public PortalNode[] Connections;
		public Vector3 Center;
		public int OpenPathId;
		public int ClosedPathId;
		public float TotalCost = float.MaxValue;
		public PortalNode Parent = null;

		public void Initialize( Portal portal, Chunk chunk, ChunkNode[] chunkNodes )
		{
			Portal = portal;
			Chunk = chunk;
			ChunkNodes = chunkNodes;
			Center = Vector3.Zero;

			for ( int i = 0; i < ChunkNodes.Length; ++i )
			{
				ChunkNodes[i].SetPortalNode( this );
				var position = ChunkNodes[i].WorldPosition;
				Center += position;
			}

			Center = Center / ChunkNodes.Length;
		}

		public List<PortalNode> FloodChunk( int floodId )
		{
			List<PortalNode> foundPortals = new();
			Chunk.Flood( floodId, ref ChunkNodes, ref foundPortals );
			return foundPortals;
		}
	}
}
