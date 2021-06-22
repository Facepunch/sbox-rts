using Gamelib.Math;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class FlowFieldChunk
	{
		public Node[,] Nodes;
		public Vector2i Index;
		public int Size;
		public Node Destination;
		public FlowField FlowField;

		public FlowFieldChunk( FlowField flowField, Vector2i index, int size )
		{
			FlowField = flowField;
			Index = index;
			Size = size;
		}

		public void UpdateNeighbours()
		{
			var size = Size;
			var nodes = Nodes;

			for ( var x = 0; x < size; x++ )
			{
				for ( var y = 0; y < size; y++ )
				{
					var node = nodes[x, y];
					GetNeighbours( node.GridIndex, GridDirection.AllDirections, node.AllNeighbours );
					GetNeighbours( node.GridIndex, GridDirection.CardinalDirections, node.CardinalNeighbours );
				}
			}
		}

		public void CreateGrid()
		{
			var worldTopLeft = FlowField.WorldTopLeft;
			var index = Index;
			var size = Size;

			Nodes = new Node[size, size];

			var startGridX = (index.x * size);
			var startGridY = (index.y * size);

			var nodeDiameter = FlowField.NodeDiameter;
			var nodeRadius = FlowField.NodeRadius;
			var nodes = Nodes;

			for ( int x = 0; x < size; x++ )
			{
				for ( int y = 0; y < size; y++ )
				{
					var worldGridX = startGridX + x;
					var worldGridY = startGridY + y;
					var worldPosition = worldTopLeft + Vector3.Forward * (worldGridX * nodeDiameter + nodeRadius);
					worldPosition += Vector3.Left * (worldGridY * nodeDiameter + nodeRadius);

					nodes[x, y] = new Node( worldPosition, new Vector2i( worldGridX, worldGridY ) );
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
			var size = Size;
			var nodes = Nodes;

			for ( var x = 0; x < size; x++ )
			{
				for ( var y = 0; y < size; y++ )
				{
					var node = nodes[x, y];
					var bestCost = node.BestCost;
					var neighbours = node.AllNeighbours;

					for ( int i = 0; i < neighbours.Length; i++ )
					{
						var neighbour = neighbours[i];

						if ( neighbour == null ) continue;

						if ( neighbour.BestCost < bestCost )
						{
							bestCost = neighbour.BestCost;
							node.BestDirection = GridDirection.GetDirectionFromVector( neighbour.GridIndex - node.GridIndex );
						}
					}
				}
			}
		}

		public void CreateCostField()
		{
			var size = Size;
			var nodes = Nodes;

			for ( var x = 0; x < size; x++ )
			{
				for ( var y = 0; y < size; y++ )
				{
					var node = nodes[x, y];
					node.BestCost = ushort.MaxValue;
					node.Cost = 1;
				}
			}
		}

		private void GetNeighbours( Vector2i nodeIndex, List<GridDirection> directions, Node[] neighbours )
		{
			var currentIndex = 0;

			for ( int i = 0; i < directions.Count; i++ )
			{
				var direction = directions[i];
				var neighbour = GetRelativeNode( nodeIndex, direction );

				if ( neighbour != null )
				{
					neighbours[currentIndex] = neighbour;
					currentIndex++;
				}
			}
		}

		private Node GetRelativeNode( Vector2i origin, Vector2i relative )
		{
			var gridSize = FlowField.GridSize;
			var output = origin + relative;

			if ( output.x < 0 || output.x >= gridSize.x || output.y < 0 || output.y >= gridSize.y )
			{
				return null;
			}
			else
			{
				return FlowField.GetNodeFromLocal( output );
			}
		}
	}
}
