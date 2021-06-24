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
			_pathfinder.Initialize();
			_pathfinder.UpdateCollisions();

			// Create a good amount of flow fields ready in the pool.
			for ( var i = 0; i < 20; i++ )
			{
				_flowFields.Enqueue( new FlowField( _pathfinder ) );
			}
		}

		public PathRequest Request( List<Vector3> destinations )
		{
			for ( int i = destinations.Count - 1; i >= 0; i-- )
			{
				var position = destinations[i];

				if ( !_pathfinder.IsAvailable( position ) )
					destinations.RemoveAt( i );
			}

			if ( destinations.Count == 0 ) return null;

			var pathRequest = GetPathRequest();
			pathRequest.FlowField.SetDestinations( destinations );
			return pathRequest;
		}

		public PathRequest Request( Vector3 destination )
		{
			if ( !_pathfinder.IsAvailable( destination ) ) return null;

			var pathRequest = GetPathRequest();
			pathRequest.FlowField.SetDestination( destination);
			return pathRequest;
		}

		public void Complete( PathRequest request )
		{
			if ( request == null || !request.IsValid() )
				return;

			request.FlowField.ResetAndClearDestination();
			_flowFields.Enqueue( request.FlowField );
			request.FlowField = null;
		}

		public void Update()
		{
			_pathfinder.Update();
		}

		private PathRequest GetPathRequest()
		{
			var isValid = _flowFields.TryDequeue( out var flowField );

			if ( !isValid )
			{
				flowField = new FlowField( _pathfinder );
			}

			return new PathRequest()
			{
				FlowField = flowField
			};
		}
    }
}
