using Gamelib.Maths;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamelib.FlowFields
{
    public static class PathManager
    {
		[ConCmd.Server( "ff_update_collisions" )]
		private static void UpdateCollisions()
		{
			foreach ( var pathfinder in All )
				_ = pathfinder.UpdateCollisions();
		}

		[ConCmd.Server( "ff_show_chunks" )]
		private static void ShowChunks()
		{
			var pathfinder = Default;
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

		[ConCmd.Server( "ff_show_portals" )]
		private static void ShowPortals( int nodeSize, int collisionSize )
		{
			var pathfinder = GetPathfinder( nodeSize, collisionSize );
			var portals = pathfinder.Portals;

			for ( var i = 0; i < portals.Count; i++ )
			{
				var portal = portals[i];
				var position = portal.GetVector( pathfinder );

				DebugOverlay.Sphere( position, 64f, Color.Green, 5f, true );
			}
		}

		[ConCmd.Server( "ff_show_gateway_nodes" )]
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
		
		[ConCmd.Server( "ff_show_collisions" )]
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

		private static Dictionary<int, Pathfinder> Pathfinders { get; set; } = new();

		public static List<Pathfinder> All { get; private set; } = new();
		public static Pathfinder Smallest { get; private set; }
		public static Pathfinder Largest { get; private set; }
		public static Pathfinder Default { get; private set; }
		public static BBox? Bounds { get; private set; }

		public static Pathfinder GetPathfinder( int nodeSize, int collisionSize )
		{
			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			if ( Pathfinders.TryGetValue( hash, out var pathfinder ) )
			{
				return pathfinder;
			}

			return Default;
		}

		public static void SetBounds( BBox bounds )
		{
			Bounds = bounds;
		}

		public static async Task Create( int nodeSize = 50, int collisionSize = 100 )
		{
			if ( !Bounds.HasValue )
				throw new Exception( "[PathManager::Create] Unable to create a Pathfinder without a world bounds value set!" );

			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			if ( !Pathfinders.ContainsKey( hash ) )
			{
				await Register( new Pathfinder( Bounds.Value, nodeSize, collisionSize ), nodeSize, collisionSize );
			}
		}

		public static void Update()
		{
			for ( var i = 0; i < All.Count; i++ )
				All[i].Update();
		}

		private static async Task Register( Pathfinder pathfinder, int nodeSize, int collisionSize )
		{
			var hash = MathUtility.HashNumbers( (short)nodeSize, (short)collisionSize );

			Pathfinders[hash] = pathfinder;

			if ( Largest == null || collisionSize > Largest.CollisionSize )
				Largest = pathfinder;

			if ( Smallest == null || collisionSize < Smallest.CollisionSize )
				Smallest = pathfinder;

			if ( Default == null )
				Default = pathfinder;

			All.Add( pathfinder );

			await pathfinder.Initialize();
		}
    }
}
