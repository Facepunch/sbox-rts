using Gamelib.Maths;
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
				var halfExtents = pathfinder.NodeExtents * chunk.Definition.Columns;

				position += halfExtents;

				DebugOverlay.Box( 10f, position - halfExtents, position + halfExtents, Color.White );
			}
		}

		[ServerCmd( "ff_show_portals" )]
		private static void ShowPortals( int nodeSize, int collisionSize )
		{
			var pathfinder = GetPathfinder( nodeSize, collisionSize );
			var portals = pathfinder.Portals;

			for ( var i = 0; i < portals.Count; i++ )
			{
				var portal = portals[i];
				var position = portal.GetVector( pathfinder );

				DebugOverlay.Sphere( position, 64f, Color.Green, true, 5f );
			}
		}

		[ServerCmd( "ff_show_gateway_nodes" )]
		private static void ShowGatewayNodes( int nodeSize, int collisionSize )
		{
			var pathfinder = GetPathfinder( nodeSize, collisionSize );
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
		private static void ShowCollisions( int nodeSize, int collisionSize )
		{
			var pathfinder = GetPathfinder( nodeSize, collisionSize );
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
		private static Pathfinder _smallest;
		private static Pathfinder _largest;
		private static Pathfinder _default;
		private static BBox? _bounds;

		public static List<Pathfinder> All { get; private set; } = new();
		public static Pathfinder Smallest => _smallest;
		public static Pathfinder Largest => _largest;
		public static Pathfinder Default => _default;
		public static BBox? Bounds => _bounds;

		public static Pathfinder GetPathfinder( int nodeSize, int collisionSize )
		{
			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			if ( _pathfinders.TryGetValue( hash, out var pathfinder ) )
			{
				return pathfinder;
			}

			return _default;
		}

		public static void SetBounds( BBox bounds )
		{
			_bounds = bounds;
		}

		public static void Create( int nodeSize = 50, int collisionSize = 100 )
		{
			if ( !_bounds.HasValue )
				throw new System.Exception( "[PathManager::Create] Unable to create a Pathfinder without a world bounds value set!" );

			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			if ( !_pathfinders.ContainsKey( hash ) )
			{
				Register( new Pathfinder( _bounds.Value, nodeSize, collisionSize ), nodeSize, collisionSize );
			}
		}

		public static void Update()
		{
			for ( var i = 0; i < All.Count; i++ )
				All[i].Update();
		}

		private static void Register( Pathfinder pathfinder, int nodeSize, int collisionSize )
		{
			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			pathfinder.Initialize();

			_pathfinders[hash] = pathfinder;

			if ( _largest == null || collisionSize > _largest.CollisionSize )
				_largest = pathfinder;

			if ( _smallest == null || collisionSize < _smallest.CollisionSize )
				_smallest = pathfinder;

			if ( _default == null )
				_default = pathfinder;

			All.Add( pathfinder );
		}
    }
}
