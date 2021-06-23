using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Portal
	{
		public FlowField FlowField = null;
		public PortalNode NodeA = null;
		public PortalNode NodeB = null;

		private Vector3 MinPos = Vector3.Zero;
		private Vector3 MaxPos = Vector3.Zero;
		private Vector3 Center = Vector3.Zero;

		public void Initialize( Chunk chunkA, ChunkNode[] nodesA, Chunk chunkB, ChunkNode[] nodesB, FlowField world )
		{
			FlowField = world;

			NodeA = new PortalNode();
			NodeA.Initialize( this, chunkA, nodesA );

			NodeB = new PortalNode();
			NodeB.Initialize( this, chunkB, nodesB );

			Center = (NodeA.Center + NodeB.Center) * 0.5f;
			MinPos = new Vector3( float.MaxValue, float.MaxValue, 0 );
			MaxPos = new Vector3( float.MinValue, float.MinValue, 0 );

			if ( NodeA.MinPos.x < MinPos.x )
			{
				MinPos.x = NodeA.MinPos.x;
			}

			if ( NodeB.MinPos.y < MinPos.y )
			{
				MinPos.y = NodeB.MinPos.y;
			}

			if ( NodeA.MaxPos.x > MaxPos.x )
			{
				MaxPos.x = NodeA.MaxPos.x;
			}

			if ( NodeB.MaxPos.y > MaxPos.y )
			{
				MaxPos.y = NodeB.MaxPos.y;
			}
		}
	}
}

