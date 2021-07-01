using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamelib.FlowFields
{
    public class PathManager
    {
		public static PathManager Instance { get; private set; }

		[ServerCmd( "ff_collisions" )]
		private static void ShowCollisions()
		{
			var pathfinder = Instance.Pathfinder;
			var chunks = pathfinder.Chunks;
			var numberOfChunks = pathfinder.Chunks.Length;

			for ( var i = 0; i < numberOfChunks; i++ )
			{
				var chunk = chunks[i];
				var collisions = chunk.Collisions;

				for ( var j = 0; j < collisions.Length; j++ )
				{
					if ( collisions[j] != NodeCollision.None )
					{
						var worldPosition = pathfinder.CreateWorldPosition( i, j );
						pathfinder.DrawBox( worldPosition, Color.White, 10f );
					}
				}
			}
		}

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
			_ = Initialize();
		}

		public void Create( int numberOfChunks, BBox bounds, float scale = 1f )
		{
			_pathfinder = new Pathfinder( numberOfChunks, bounds, scale );
			_ = Initialize();
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
			pathRequest.FlowField.SetDestination( destination );

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

		private async Task Initialize()
		{
			_pathfinder.Initialize();

			await _pathfinder.UpdateCollisions();

			// Create a good amount of flow fields ready in the pool.
			for ( var i = 0; i < 20; i++ )
			{
				await Task.Delay( 30 );

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
