using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Portal
	{
		public FlowField FlowField = null;
		public PortalNode NodeA = null;
		public PortalNode NodeB = null;

		public void Initialize( Chunk chunkA, ChunkNode[] nodesA, Chunk chunkB, ChunkNode[] nodesB, FlowField world )
		{
			FlowField = world;

			NodeA = new PortalNode();
			NodeA.Initialize( this, chunkA, nodesA );

			NodeB = new PortalNode();
			NodeB.Initialize( this, chunkB, nodesB );
		}
	}
}

