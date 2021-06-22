using Gamelib.Math;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class FlowField
	{
		public FlowFieldChunk[,] Chunks;
		public Vector2i GridSize;
		public float NodeRadius;
		public Node Destination;
		public float NodeDiameter;
		public int ChunkSize;
		public Vector2 WorldSize;
		public Vector3 WorldTopLeft;

		public FlowField( float nodeRadius, Vector2i gridSize, int chunkSize )
		{
			NodeRadius = nodeRadius;
			NodeDiameter = NodeRadius * 2f;
			ChunkSize = chunkSize;
			GridSize = gridSize;
		}

		public void CreateGrid()
		{
			var chunkSize = ChunkSize;
			var chunksX = GridSize.x / chunkSize;
			var chunksY = GridSize.y / chunkSize;

			Chunks = new FlowFieldChunk[
				chunksX,
				chunksY
			];

			var worldSizeX = GridSize.x * NodeDiameter;
			var worldSizeY = GridSize.y * NodeDiameter;

			WorldSize = new Vector2(
				worldSizeX,
				worldSizeY
			);

			WorldTopLeft = Vector3.Zero - Vector3.Forward * worldSizeX / 2f - Vector3.Left * worldSizeY / 2f;

			var chunks = Chunks;

			for ( int x = 0; x < chunksX; x++ )
			{
				for ( int y = 0; y < chunksY; y++ )
				{
					var chunk = new FlowFieldChunk( this, new Vector2i( x, y ), chunkSize );
					chunk.CreateGrid();
					chunks[x, y] = chunk;
				}
			}

			for ( var x = 0; x < chunksX; x++ )
			{
				for ( var y = 0; y < chunksY; y++ )
				{
					var chunk = chunks[x, y];
					chunk.UpdateNeighbours();
				}
			}
		}

		public Node GetNodeFromWorld( Vector3 worldPosition )
		{
			var gridSize = GridSize;
			var nodeDiameter = NodeDiameter;
			var worldSizeX = gridSize.x * nodeDiameter;
			var worldSizeY = gridSize.y * nodeDiameter;

			var px = ((worldPosition.x + worldSizeX / 2f) / worldSizeX);
			var py = ((worldPosition.y + worldSizeY / 2f) / worldSizeY);

			px = px.Clamp( 0f, 1f );
			py = py.Clamp( 0f, 1f );

			var fx = gridSize.x * px;
			var x = fx.FloorToInt().Clamp( 0, gridSize.x - 1 );

			var fy = gridSize.y * py;
			var y = fy.FloorToInt().Clamp( 0, gridSize.y - 1 );

			return GetNodeFromLocal( new Vector2i( x, y ) );
		}

		public Node GetNodeFromLocal( Vector2i position )
		{
			var chunk = GetChunkFromLocal( position );
			var nodeX = position.x % ChunkSize;
			var nodeY = position.y % ChunkSize;
			return chunk.Nodes[nodeX, nodeY];
		}

		public FlowFieldChunk GetChunkFromLocal( Vector2i position )
		{
			var x = ((float)(position.x / ChunkSize)).CeilToInt();
			var y = ((float)(position.y / ChunkSize)).CeilToInt();
			return Chunks[x, y];
		}

		public void CreateCostField()
		{
			var chunkSize = ChunkSize;
			var gridSize = GridSize;
			var chunksX = gridSize.x / chunkSize;
			var chunksY = gridSize.y / chunkSize;
			var chunks = Chunks;

			for ( var x = 0; x < chunksX; x++ )
			{
				for ( var y = 0; y < chunksY; y++ )
				{
					var chunk = chunks[x, y];
					chunk.CreateCostField();
				}
			}
		}

		public void CreateIntegrationField( Node destination )
		{
			Destination = destination;
			Destination.Cost = 0;
			Destination.BestCost = 0;

			var nodesToCheck = new Queue<Node>();

			nodesToCheck.Enqueue( Destination );

			while ( nodesToCheck.Count > 0 )
			{
				var currentNode = nodesToCheck.Dequeue();
				var neighbours = currentNode.CardinalNeighbours;

				for ( int i = 0; i < neighbours.Length; i++ )
				{
					var neighbour = neighbours[i];

					if ( neighbour == null ) continue;
					if ( neighbour.Cost == byte.MaxValue ) continue;

					if ( neighbour.Cost + currentNode.BestCost < neighbour.BestCost )
					{
						neighbour.BestCost = (ushort)(neighbour.Cost + currentNode.BestCost);
						nodesToCheck.Enqueue( neighbour );
					}
				}
			}
		}

		public void CreateFlowField()
		{
			var chunkSize = ChunkSize;
			var gridSize = GridSize;
			var chunksX = gridSize.x / chunkSize;
			var chunksY = gridSize.y / chunkSize;
			var chunks = Chunks;

			for ( var x = 0; x < chunksX; x++ )
			{
				for ( var y = 0; y < chunksY; y++ )
				{
					var chunk = chunks[x, y];
					chunk.CreateFlowField();
				}
			}
		}
	}
}
