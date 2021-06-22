using Gamelib.Math;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class FlowField
	{
		public Node[,] Nodes { get; private set; }
		public Vector2i GridSize { get; private set; }
		public float NodeRadius { get; private set; }
		public Node Destination { get; private set; }
		private float NodeDiameter { get; set; }

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

			for ( int x = 0; x < GridSize.x; x++ )
			{
				for ( int y = 0; y < GridSize.y; y++ )
				{
					var worldPosition = bottomLeft + Vector3.Forward * (x * NodeDiameter + NodeRadius) + Vector3.Left * (y * NodeDiameter + NodeRadius);
					Nodes[x, y] = new Node( worldPosition, new Vector2i( x, y ) );
				}
			}
		}

		public void CreateCostField()
		{
			//var nodeHalfExtents = Vector3.One * NodeRadius;

			foreach ( var node in Nodes )
			{
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
				node.IncreaseCost( 1 );
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
				var neighbours = GetNeighbours( currentNode.GridIndex, GridDirection.CardinalDirections );

				foreach ( var neighbour in neighbours )
				{
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
			foreach ( var node in Nodes )
			{
				var neighbours = GetNeighbours( node.GridIndex, GridDirection.AllDirections );
				var bestCost = node.BestCost;

				foreach ( var neighbour in neighbours )
				{
					if ( neighbour.BestCost < bestCost )
					{
						bestCost = neighbour.BestCost;
						node.BestDirection = GridDirection.GetDirectionFromVector( neighbour.GridIndex - node.GridIndex );
					}
				}
			}
		}

		private List<Node> GetNeighbours( Vector2i nodeIndex, List<GridDirection> directions )
		{
			var neighbours = new List<Node>();

			foreach ( var direction in directions )
			{
				var neighbour = GetRelativeNode( nodeIndex, direction );

				if ( neighbour != null )
				{
					neighbours.Add( neighbour );
				}
			}

			return neighbours;
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
			var worldSizeX = GridSize.x * NodeDiameter;
			var worldSizeY = GridSize.y * NodeDiameter;

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
