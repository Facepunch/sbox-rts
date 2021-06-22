using Gamelib.Math;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class FlowField
	{
		public Node[,] Nodes;
		public Vector2i GridSize;
		public float NodeRadius;
		public Node Destination;
		private float NodeDiameter;

		public FlowField( float nodeRadius, Vector2i gridSize )
		{
			NodeRadius = nodeRadius;
			NodeDiameter = NodeRadius * 2f;
			GridSize = gridSize;
		}

		public void CreateGrid()
		{
			Nodes = new Node[GridSize.x, GridSize.y];

			var worldSizeX = GridSize.x * NodeDiameter;
			var worldSizeY = GridSize.y * NodeDiameter;
			var bottomLeft = Vector3.Zero - Vector3.Forward * worldSizeX / 2 - Vector3.Left * worldSizeY / 2;
			var nodeDiameter = NodeDiameter;
			var nodeRadius = NodeRadius;
			var gridSize = GridSize;
			var nodes = Nodes;

			for ( int x = 0; x < gridSize.x; x++ )
			{
				for ( int y = 0; y < gridSize.y; y++ )
				{
					var worldPosition = bottomLeft + Vector3.Forward * (x * nodeDiameter + nodeRadius) + Vector3.Left * (y * nodeDiameter + nodeRadius);
					nodes[x, y] = new Node( worldPosition, new Vector2i( x, y ) );
				}
			}

			for ( var x = 0; x < gridSize.x; x++ )
			{
				for ( var y = 0; y < gridSize.y; y++ )
				{
					var node = nodes[x, y];

					GetNeighbours( node.GridIndex, GridDirection.AllDirections, node.AllNeighbours );
					GetNeighbours( node.GridIndex, GridDirection.CardinalDirections, node.CardinalNeighbours );
				}
			}
		}

		public void CreateCostField()
		{
			//var nodeHalfExtents = Vector3.One * NodeRadius;
			var gridSize = GridSize;
			var nodes = Nodes;

			for ( var x = 0; x < gridSize.x; x++ )
			{
				for ( var y = 0; y < gridSize.y; y++ )
				{
					var node = nodes[x, y];

					/*
					var pointContents = Physics.GetPointContents( node.WorldPosition );

					if ( pointContents != CollisionLayer.Empty )
					{
						node.IncreaseCost( 255 );
					}
					else
					{
						// TODO: Maybe we want to increase the cost here for some other obstacle.
					}
					*/

					node.BestCost = ushort.MaxValue;
					node.Cost = 1;
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
			var gridSize = GridSize;
			var nodes = Nodes;

			for ( var x = 0; x < gridSize.x; x++ )
			{
				for ( var y = 0; y < gridSize.y; y++ )
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
			var finalPos = origin + relative;

			if ( finalPos.x < 0 || finalPos.x >= GridSize.x || finalPos.y < 0 || finalPos.y >= GridSize.y )
			{
				return null;
			}
			else
			{
				return Nodes[finalPos.x, finalPos.y];
			}
		}

		public Node GetNodeFromWorld( Vector3 worldPosition )
		{
			var nodeDiameter = NodeDiameter;
			var gridSize = GridSize;
			var worldSizeX = gridSize.x * nodeDiameter;
			var worldSizeY = gridSize.y * nodeDiameter;

			var px = ((worldPosition.x + worldSizeX / 2f) / worldSizeX);
			var py = ((worldPosition.y + worldSizeY / 2f) / worldSizeY);

			px = px.Clamp( 0f, 1f );
			py = py.Clamp( 0f, 1f );

			var fx = GridSize.x * px;
			var x = fx.FloorToInt().Clamp( 0, GridSize.x - 1 );

			var fy = GridSize.y * py;
			var y = fy.FloorToInt().Clamp( 0, GridSize.y - 1 );

			return Nodes[x, y];
		}
	}
}
