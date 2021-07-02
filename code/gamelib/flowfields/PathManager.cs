using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamelib.FlowFields
{
    public class PathManager
    {
		public static PathManager Instance { get; private set; }

		[ServerCmd( "ff_update_collisions" )]
		private static void UpdateCollisions()
		{
			foreach ( var pathfinder in Instance.All )
				pathfinder.UpdateCollisions();
		}

		[ServerCmd( "ff_show_chunks" )]
		private static void ShowChunks()
		{
			var pathfinder = Instance.Default;
			var chunks = pathfinder.Chunks;
			var numberOfChunks = pathfinder.Chunks.Length;

			for ( var i = 0; i < numberOfChunks; i++ )
			{
				var chunk = chunks[i];
				var position = pathfinder.GetLocalChunkPosition( chunk.Index ) - pathfinder.PositionOffset;
				var halfExtents = pathfinder.HalfExtents * chunk.Definition.Columns;

				position += halfExtents;

				DebugOverlay.Box( 10f, position - halfExtents, position + halfExtents, Color.White );
			}
		}

		[ServerCmd( "ff_show_gateway_nodes" )]
		private static void ShowGatewayNodes( int size )
		{
			var pathfinder = Instance.GetPathfinder( size );
			var chunks = pathfinder.Chunks;
			var numberOfChunks = pathfinder.Chunks.Length;

			for ( var i = 0; i < numberOfChunks; i++ )
			{
				var chunk = chunks[i];
				
				foreach ( var gateway in chunk.GetGateways() )
				{
					foreach ( var node in gateway.Nodes )
					{
						var worldPosition = pathfinder.CreateWorldPosition( chunk.Index, node );
						pathfinder.DrawBox( worldPosition, Color.Green, 10f );
					}
				}
			}
		}
		
		[ServerCmd( "ff_show_collisions" )]
		private static void ShowCollisions( int size )
		{
			var pathfinder = Instance.GetPathfinder( size );
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

		private Dictionary<int, Pathfinder> _pathfinders = new();
		private Pathfinder _default;

		public Pathfinder Default => _default;
		public List<Pathfinder> All { get; private set; } = new();

		public PathManager()
		{
			Instance = this;
		}

		public Pathfinder GetPathfinder( int size )
		{
			if ( _pathfinders.TryGetValue( size, out var pathfinder ) )
			{
				return pathfinder;
			}

			return _default;
		}

        public void Create( int numberOfChunks, int chunkSize, int nodeSize = 100 )
		{
			Register( new Pathfinder( numberOfChunks, chunkSize, nodeSize ), nodeSize );
		}

		public void Create( int numberOfChunks, BBox bounds, int nodeSize = 100 )
		{
			Register( new Pathfinder( numberOfChunks, bounds, nodeSize ), nodeSize );
		}

		public void Update()
		{
			for ( var i = 0; i < All.Count; i++ )
				All[i].Update();
		}

		private void Register( Pathfinder pathfinder, int nodeSize )
		{
			pathfinder.Initialize();
			_pathfinders[nodeSize] = pathfinder;

			if ( _default == null )
				_default = pathfinder;

			All.Add( pathfinder );
		}
    }
}
