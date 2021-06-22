using Gamelib.Math;
using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Node
	{
		public Vector3 WorldPosition;
		public Vector2i GridIndex;
		public byte Cost;
		public ushort BestCost;
		public GridDirection BestDirection;
		public FlowFieldChunk Chunk;
		public Node[] AllNeighbours;
		public Node[] CardinalNeighbours;

		public Node( FlowFieldChunk chunk, Vector3 worldPosition, Vector2i gridIndex )
		{
			WorldPosition = worldPosition;
			GridIndex = gridIndex;
			Cost = 1;
			Chunk = chunk;
			BestCost = ushort.MaxValue;
			BestDirection = GridDirection.None;
			AllNeighbours = new Node[9];
			CardinalNeighbours = new Node[4];
		}

		public void IncreaseCost( int amount )
		{
			if ( Cost == byte.MaxValue ) return;

			if ( amount + Cost >= 255 )
				Cost = byte.MaxValue;
			else
				Cost += (byte)amount;
		}
	}
}
