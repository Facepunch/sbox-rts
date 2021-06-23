using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class AStar
	{
		public PriorityQueue<PortalNode> OpenList = new PriorityQueue<PortalNode>();

		private Vector3 StartPosition;
		private Vector3 GoalPosition;

		public List<PortalNode> FindPath( int pathId, FlowField world, Vector3 startPosition, Vector3 goalPosition )
		{
			StartPosition = startPosition;
			GoalPosition = goalPosition;

			var startingPortals = new List<PortalNode>();
			var goalPortals = new List<PortalNode>();

			var startingChunk = world.GetChunkAtWorld( startPosition );
			var goalChunk = world.GetChunkAtWorld( goalPosition );

			var startNode = startingChunk.GetNodeAtWorld( startPosition );
			var goalNode = goalChunk.GetNodeAtWorld( goalPosition );

			var startNodes = new ChunkNode[] { startNode };
			startingChunk.Flood( ++FlowField.CurrentFloodPathId, ref startNodes, ref startingPortals );

			var goalNodes = new ChunkNode[] { goalNode };
			goalChunk.Flood( ++FlowField.CurrentFloodPathId, ref goalNodes, ref goalPortals );

			OpenList.Clear();

			for ( int i = 0; i < startingPortals.Count; ++i )
			{
				AddToOpen( pathId, startingPortals[i], null );
			}

			PortalNode finalNode = null;
			var foundGoalNode = false;

			while ( OpenList.Count > 0 && !foundGoalNode )
			{
				var node = OpenList.Dequeue();

				AddToClosed( pathId, node );

				var neighbours = node.Connections;

				for ( int i = 0; i < neighbours.Length; ++i )
				{
					if ( neighbours[i].OpenPathId != pathId )
					{
						if ( neighbours[i].ClosedPathId != pathId )
						{
							AddToOpen( pathId, neighbours[i], node );

							if ( FoundGoalNode( ref neighbours[i], ref goalPortals ) )
							{
								finalNode = neighbours[i];
								foundGoalNode = true;
								break;
							}
						}
						else
						{
							var prevCost = GetCost( neighbours[i] );
							var potentialCost = node.TotalCost + node.Center.Distance( neighbours[i].Center );

							if ( prevCost > potentialCost )
							{
								AddToOpen( pathId, neighbours[i], node );
							}
						}
					}
				}
			}

			var path = new List<PortalNode>();
			var parentNode = finalNode;

			while ( parentNode != null )
			{
				parentNode.FloodChunk( ++FlowField.CurrentPathId );
				path.Add( parentNode );
				parentNode = parentNode.Parent;
			}

			return path;
		}

		private bool FoundGoalNode( ref PortalNode nodeToTest, ref List<PortalNode> goalPortals )
		{
			for ( int j = 0; j < goalPortals.Count; ++j )
			{
				if ( nodeToTest == goalPortals[j] )
				{
					return true;
				}
			}

			return false;
		}

		private float GetCost( PortalNode node )
		{
			float g = node.TotalCost;
			float h = node.Center.Distance( GoalPosition );
			float f = g + h;

			return f;
		}

		private void AddToOpen( int pathId, PortalNode node, PortalNode parent )
		{
			var f = GetCost( node );
			node.OpenPathId = pathId;
			node.TotalCost = 0.0f;
			node.Parent = parent;

			if ( parent == null )
			{
				node.TotalCost = StartPosition.Distance( node.Center );
			}
			else
			{
				node.TotalCost = node.Parent.TotalCost;
				node.TotalCost += node.Parent.Center.Distance( node.Center );
			}

			OpenList.Enqueue( node, f );
		}

		private void AddToClosed( int pathId, PortalNode node )
		{
			node.ClosedPathId = pathId;
		}
	}
}
