using Gamelib.Math;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public struct Node
	{
		public Vector3 WorldPosition;
		public Vector2i GridIndex;
		public byte Cost;
		public ushort BestCost;
		public GridDirection BestDirection;
		public Vector2i[] AllNeighbours;
		public Vector2i[] CardinalNeighbours;

		public Node( Vector3 worldPosition, Vector2i gridIndex )
		{
			WorldPosition = worldPosition;
			GridIndex = gridIndex;
			Cost = 1;
			BestCost = ushort.MaxValue;
			BestDirection = GridDirection.None;
			AllNeighbours = new Vector2i[8];
			CardinalNeighbours = new Vector2i[4];
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
