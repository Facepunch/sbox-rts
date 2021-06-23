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
		public Vector3 MinPos;
		public Vector3 MaxPos;
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
			MinPos = new Vector3( float.MaxValue, float.MaxValue, 0.0f );
			MaxPos = new Vector3( float.MinValue, float.MinValue, 0.0f );

			for ( int i = 0; i < ChunkNodes.Length; ++i )
			{
				ChunkNodes[i].SetPortalNode( this );
				var position = ChunkNodes[i].WorldPosition;
				MakeBoundingBox( position );
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

		private void MakeBoundingBox( Vector3 position )
		{
			if ( position.x < MinPos.x )
			{
				MinPos.x = position.x;
			}

			if ( position.y < MinPos.y )
			{
				MinPos.y = position.y;
			}

			if ( position.x > MaxPos.x )
			{
				MaxPos.x = position.x;
			}

			if ( position.y > MaxPos.y )
			{
				MaxPos.y = position.y;
			}
		}
	}
}
