using Gamelib.FlowFields.Grid;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public class Pathfinding
    {
		private Queue<FlowField> _flowFields = new();
		private Pathfinder _pathfinder;

		public Pathfinder Pathfinder => _pathfinder;

        public void Initialize( int numberOfChunks, int chunkSize, float scale = 1f )
		{
			_pathfinder = new Pathfinder( numberOfChunks, chunkSize, scale );
			_pathfinder.Init();
			_pathfinder.UpdateCollisions();
		}

		public PathRequest Request( Vector3 from, Vector3 to )
		{
			var isValid = _flowFields.TryDequeue( out var flowField );

			if ( !isValid )
			{
				flowField = new FlowField( _pathfinder );
			}

			var gridPosition = _pathfinder.CreateWorldPosition( to );
			var worldIndex = new List<int> { gridPosition.WorldIndex };

			flowField.ResetDestination();
			flowField.CreateDestinationGateways( worldIndex );
			flowField.UpdatePaths();

			return new PathRequest()
			{
				FlowField = flowField
			};
		}

		public void Complete( PathRequest request )
		{
			if ( request == null || !request.IsValid() )
				return;

			request.FlowField.ResetDestination();
			_flowFields.Enqueue( request.FlowField );
			request.FlowField = null;
		}

		public void Update()
		{
			_pathfinder.Update();
		}
    }
}
