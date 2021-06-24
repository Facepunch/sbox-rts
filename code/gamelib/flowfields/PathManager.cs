using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public class PathManager
    {
		public static PathManager Instance { get; private set; }

		private Queue<FlowField> _flowFields = new();
		private Pathfinder _pathfinder;

		public Pathfinder Pathfinder => _pathfinder;

		public PathManager()
		{
			Instance = this;
		}

        public void Create( int numberOfChunks, int chunkSize, float scale = 1f )
		{
			_pathfinder = new Pathfinder( numberOfChunks, chunkSize, scale );
			Initialize();
		}

		public void Create( int numberOfChunks, BBox bounds, float scale = 1f )
		{
			_pathfinder = new Pathfinder( numberOfChunks, bounds, scale );
			Initialize();
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

			var pathRequest = GetRequest();
			pathRequest.FlowField.SetDestinations( destinations );
			return pathRequest;
		}

		public PathRequest Request( Vector3 destination )
		{
			if ( !_pathfinder.IsAvailable( destination ) ) return null;

			var pathRequest = GetRequest();
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

		private void Initialize()
		{
			_pathfinder.Initialize();
			_pathfinder.UpdateCollisions();

			// Create a good amount of flow fields ready in the pool.
			for ( var i = 0; i < 20; i++ )
			{
				_flowFields.Enqueue( new FlowField( _pathfinder ) );
			}
		}

		private PathRequest GetRequest()
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
