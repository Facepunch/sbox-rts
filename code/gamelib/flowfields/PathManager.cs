using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public static class PathManager
    {
		[ServerCmd( "ff_update_collisions" )]
		private static void UpdateCollisions()
		{
			foreach ( var pathfinder in All )
				pathfinder.UpdateCollisions();
		}

		[ServerCmd( "ff_show_chunks" )]
		private static void ShowChunks()
		{
			var pathfinder = _default;
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

		[ServerCmd( "ff_show_portals" )]
		private static void ShowPortals( int size )
		{
			var pathfinder = GetPathfinder( size );
			var portals = pathfinder.Portals;

			for ( var i = 0; i < portals.Count; i++ )
			{
				var portal = portals[i];
				var position = portal.GetVector( pathfinder );

				DebugOverlay.Sphere( position, 64f, Color.Green, true, 5f );
			}
		}

		[ServerCmd( "ff_show_gateway_nodes" )]
		private static void ShowGatewayNodes( int size )
		{
			var pathfinder = GetPathfinder( size );
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
			var pathfinder = GetPathfinder( size );
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

		private static Dictionary<int, Pathfinder> _pathfinders = new();
		private static Pathfinder _default;

		public static List<Pathfinder> All { get; private set; } = new();
		public static Pathfinder Default => _default;

		public static Pathfinder GetPathfinder( int size )
		{
			if ( _pathfinders.TryGetValue( size, out var pathfinder ) )
			{
				return pathfinder;
			}

			return _default;
		}

        public static void Create( int numberOfChunks, int chunkSize, int nodeSize = 100 )
		{
			Register( new Pathfinder( numberOfChunks, chunkSize, nodeSize ), nodeSize );
		}

		public static void Create( int numberOfChunks, BBox bounds, int nodeSize = 100 )
		{
			Register( new Pathfinder( numberOfChunks, bounds, nodeSize ), nodeSize );
		}

		public static void Update()
		{
			for ( var i = 0; i < All.Count; i++ )
				All[i].Update();
		}

		private static void Register( Pathfinder pathfinder, int nodeSize )
		{
			pathfinder.Initialize();
			_pathfinders[nodeSize] = pathfinder;

			if ( _default == null )
				_default = pathfinder;

			All.Add( pathfinder );
		}
    }
}
