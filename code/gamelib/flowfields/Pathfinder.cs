using System;
using System.Collections.Generic;
using Gamelib.Maths;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Sandbox;
using Gamelib.FlowFields.Entities;
using System.Threading.Tasks;

namespace Gamelib.FlowFields
{
    public class Pathfinder
    {
		public static event Action<Pathfinder, List<int>> OnWorldChanged;

        public readonly List<Portal> Portals = new();

		private readonly List<GridWorldPosition> InternalCollisionBuffer = new();
		private readonly List<int> InternalChunkBuffer = new();
		private readonly Queue<FlowField> FlowFieldQueue = new();

		[ConVar.Server( "ff_debug", Saved = true )]
		public static bool Debug { get; set; }

		private GridDefinition InternalNumberOfChunks = new( 10, 10 );
        private GridDefinition InternalChunkGridSize = new( 10, 10 );
        private GridDefinition InternalWorldGridSize = new( 100, 100 );
        private PhysicsBody InternalPhysicsBody;
        private Vector3 InternalPositionOffset;
        private Vector3 InternalCenterOffset;
        private Vector3 InternalCollisionExtents;
        private Vector3 InternalNodeExtents;
		private float[] InternalHeightMap;
        private Chunk[] InternalChunks;
        private int WaitForPhysicsUpdate = 3;
        private int InternalCollisionSize = 100;
        private int InternalNodeSize = 50;

		public PhysicsBody PhysicsBody => InternalPhysicsBody;
		public Vector3 PositionOffset => InternalPositionOffset;
		public Vector3 CenterOffset => InternalCenterOffset;
		public Vector3 CollisionExtents => InternalCollisionExtents;
		public Vector3 NodeExtents => InternalNodeExtents;
		public float HeightThreshold { get; set; } = 60f;
		public Vector3 Origin { get; private set; }

		public GridDefinition ChunkGridSize => InternalChunkGridSize;
		public GridDefinition NumberOfChunks => InternalNumberOfChunks;
		public GridDefinition WorldGridSize => InternalWorldGridSize;
		public Chunk[] Chunks => InternalChunks;
		public int CollisionSize => InternalCollisionSize;
		public int NodeSize => InternalNodeSize;

		public Pathfinder( int numberOfChunks, int chunkGridSize, int nodeSize = 50, int collisionSize = 100 )
		{
			SetupSize( numberOfChunks, chunkGridSize, nodeSize, collisionSize );
		}

		public Pathfinder( BBox bounds, int nodeSize = 50, int collisionSize = 100 )
		{
			var delta = bounds.Maxs - bounds.Mins;
			var width = delta.x;
			var height = delta.y;
			var squareSize = (MathF.Ceiling( Math.Max( width, height ) / 1000f ) * 1000f) + 1000f;
			var numberOfChunks = squareSize / nodeSize / 10;
			var chunkSize = squareSize / nodeSize / numberOfChunks;

			Origin = bounds.Center;

			SetupSize( numberOfChunks.CeilToInt(), chunkSize.CeilToInt(), nodeSize, collisionSize );
		}

		public void Update()
        {
            if (WaitForPhysicsUpdate > 0)
            {
                WaitForPhysicsUpdate--;
                return;
            }

            ProcessBuffers();
        }

		public PathRequest Request( List<Vector3> destinations )
		{
			for ( int i = destinations.Count - 1; i >= 0; i-- )
			{
				var position = destinations[i];

				if ( !IsAvailable( position ) )
					destinations.RemoveAt( i );
			}

			if ( destinations.Count == 0 ) return null;

			var pathRequest = GetRequest();
			pathRequest.FlowField.SetDestinations( destinations );
			return pathRequest;
		}

		public PathRequest Request( Vector3 destination )
		{
			if ( !IsAvailable( destination ) ) return null;

			var pathRequest = GetRequest();
			pathRequest.FlowField.SetDestination( destination );

			return pathRequest;
		}

		public void Complete( PathRequest request )
		{
			if ( request == null || !request.IsValid() )
				return;

			request.FlowField.ResetAndClearDestination();
			FlowFieldQueue.Enqueue( request.FlowField );
			request.FlowField = null;
		}

		public async Task UpdateCollisions()
        {
            for ( var index = 0; index < WorldGridSize.Size; index++ )
            {
				UpdateCollisions( index );
			}

			await GameTask.Delay( 50 );
		}

		public TraceResult GetCollisionTrace( Vector3 position, int worldIndex )
		{
			var transform = InternalPhysicsBody.Transform;
			var heightMap = InternalHeightMap[worldIndex];

			transform.Position = (position + InternalCenterOffset).WithZ( InternalCollisionExtents.z + heightMap + 5f );

			var trace = Trace.Sweep( InternalPhysicsBody, transform, transform )
				.EntitiesOnly()
				.WithoutTags( "ff_ignore" )
				.Run();

			return trace;
		}

		public bool IsCollisionAt( Vector3 position, int worldIndex )
		{
			var trace = GetCollisionTrace( position, worldIndex );
			return trace.Hit || trace.StartedSolid;
		}

		public bool IsCollisionAt( Vector3 position, int worldIndex, out Entity entity )
		{
			var trace = GetCollisionTrace( position, worldIndex );
			entity = trace.Entity;
			return trace.Hit || trace.StartedSolid;
		}

		public bool IsAvailable( GridWorldPosition position )
		{
			var chunk = GetChunk( position.ChunkIndex );
			if ( chunk == null ) return false;
			return !chunk.IsImpassable( position.NodeIndex );
		}

		public bool IsAvailable( Vector3 position )
		{
			return IsAvailable( CreateWorldPosition( position ) );
		}

		public void UpdateCollisions( int worldIndex )
		{
			var chunk = GetChunk( GetChunkIndex( worldIndex ) );
			var nodeIndex = GetNodeIndex( worldIndex );
			var position = GetPosition( worldIndex );

			if ( chunk.GetCollision( nodeIndex ) == NodeCollision.Static )
			{
				// Static collisions never change, let's not update this node.
				return;
			}

			if ( IsCollisionAt( position, worldIndex, out var entity ) )
			{
				if ( !entity.IsValid() || entity.IsWorld || entity is FlowFieldBlocker )
					chunk.SetCollision( nodeIndex, NodeCollision.Static );
				else
					chunk.SetCollision( nodeIndex );
			}
			else
			{
				chunk.RemoveCollision( nodeIndex );
			}
		}

		public void DrawBox( Vector3 position, int worldIndex, Color? color = null, float duration = 1f )
		{
			if ( !Debug ) return;

			var transform = InternalPhysicsBody.Transform;
			transform.Position = (position + InternalCenterOffset).WithZ( InternalNodeExtents.z + InternalHeightMap[worldIndex] );
			DebugOverlay.Box( transform.Position, -InternalCollisionExtents, InternalCollisionExtents, Color.Red, duration, false );
			DebugOverlay.Box( transform.Position, -InternalNodeExtents, InternalNodeExtents, color.HasValue ? color.Value : Color.White, duration, false );
		}

		public void DrawBox( int worldIndex, Color? color = null, float duration = 1f )
		{
			DrawBox( GetPosition( worldIndex ), worldIndex, color, duration );
		}

		public void DrawBox( GridWorldPosition position, Color? color = null, float duration = 1f )
		{
			DrawBox( GetPosition( position ), position.WorldIndex, color, duration );
		}

        public async Task Initialize()
        {
			InternalPositionOffset = Origin + new Vector3(
				InternalWorldGridSize.Columns * InternalNodeSize / 2f,
				InternalWorldGridSize.Rows * InternalNodeSize / 2f,
				0f
			);

			InternalCenterOffset = new Vector3(
				NodeSize / 2f,
				NodeSize / 2f,
				0f
			);

			InternalChunkBuffer.Clear();

			CreateChunks();

			await CreateHeightMap();
			await UpdateCollisions();

			ConnectPortals();

			// Create a good amount of flow fields ready in the pool.
			for ( var i = 0; i < 10; i++ )
			{
				FlowFieldQueue.Enqueue( new FlowField( this ) );
			}
		}

		public async Task CreateHeightMap()
		{
			var worldSizeLength = InternalWorldGridSize.Size;

			InternalHeightMap = new float[worldSizeLength];

			var heightFlags = new bool[worldSizeLength];
			var collisionFlags = new bool[worldSizeLength];
			var gridDirections = GridUtility.GetGridDirections( true );

			for ( var index = 0; index < worldSizeLength; index++ )
			{
				DoHeightMapTrace( index, heightFlags );

				var worldPosition = CreateWorldPosition( index );
				var thisHeight = InternalHeightMap[index];
				var chunk = GetChunk( worldPosition.ChunkIndex );

				for ( var j = 0; j < gridDirections.Count; j++ )
				{
					if ( !CheckHeightDifference( index, thisHeight, gridDirections[j], heightFlags, collisionFlags ) )
					{
						chunk.SetCollision( worldPosition.NodeIndex, NodeCollision.Static );
						collisionFlags[index] = true;
						break;
					}
				}
			}

			await GameTask.Delay( 50 );
		}

		private bool CheckHeightDifference( int index, float height, GridDirection direction, bool[] heightFlags, bool[] collisionFlags )
		{
			var neighbor = GridUtility.GetNeighborIndex( index, direction, InternalWorldGridSize );

			if ( neighbor != int.MinValue ) //&& !collisionFlags[neighbor] )
			{
				DoHeightMapTrace( neighbor, heightFlags );

				var neighborHeight = InternalHeightMap[neighbor];

				// Check if there's a large enough height distance.
				if ( Math.Abs( neighborHeight - height ) > HeightThreshold )
				{
					//DrawBox( index, Color.Magenta, 120f );
					return false;
				}
			}

			return true;
		}

		private void DoHeightMapTrace( int index, bool[] heightFlags )
		{
			if ( heightFlags[index] ) return;

			var position = GetPosition( index );
			InternalHeightMap[index] = HeightCache.GetHeight( position );

			heightFlags[index] = true;
		}

		public void ConnectPortals()
        {
            Portals.Clear();

            for ( var i = 0; i < InternalNumberOfChunks.Size; i++ )
			{
				var chunk = GetChunk( i );
				chunk.ClearGateways();
			}

            for ( var i = 0; i < InternalNumberOfChunks.Size; i++ )
            {
                CreatePortalsBetweenChunks( i, GridDirection.Up );
                CreatePortalsBetweenChunks( i, GridDirection.Right );
            }

			for ( int i = 0; i < InternalChunks.Length; i++ )
			{
				var chunk = InternalChunks[i];
				chunk.ConnectGateways();
			}
        }

        public void ClearChunks()
        {
            InternalChunks = null;
        }

        public void SetCost( GridWorldPosition position, byte cost = byte.MaxValue )
        {
            GetChunk( position.ChunkIndex ).SetCost( position.NodeIndex, cost );
        }

        public int GetCost( GridWorldPosition position )
        {
            return GetCost( position.ChunkIndex, position.NodeIndex );
        }

        public int GetCost( int chunk, int node )
        {
            if ( chunk == int.MinValue )
                return 255;

            return GetChunk( chunk ).GetCost( node );
        }

		public void UpdateCollisions( Vector3 position, float radius )
		{
			UpdateCollisions( position, Convert.ToInt32( radius / NodeSize ) );
		}

		public void GetGridPositions( Vector3 position, int gridSize, List<Vector3> output, bool onlyAvailable = false )
		{
			var grid = new GridDefinition( gridSize * 2 + 1, gridSize * 2 + 1 );
			var worldPivotPosition = CreateWorldPosition( position );
			var translation = new GridConverter( grid, WorldGridSize, grid.Size / 2, worldPivotPosition.WorldIndex );

			for ( var i = 0; i < grid.Size; i++ )
			{
				var gridPosition = GetPosition( translation.Global( i ) );

				if ( !onlyAvailable || IsAvailable( gridPosition ) )
					output.Add( gridPosition );
			}
		}

		public void GetGridPositions( Vector3 position, float radius, List<Vector3> output, bool onlyAvailable = false )
		{
			GetGridPositions( position, Convert.ToInt32( radius / NodeSize ), output, onlyAvailable );
		}

		public void GetGridPositions( Vector3 position, int gridSize, List<GridWorldPosition> output, bool onlyAvailable = false )
		{
			var grid = new GridDefinition( gridSize * 2 + 1, gridSize * 2 + 1 );
			var worldPivotPosition = CreateWorldPosition( position );
			var translation = new GridConverter( grid, WorldGridSize, grid.Size / 2, worldPivotPosition.WorldIndex );

			for ( var i = 0; i < grid.Size; i++ )
			{
				var gridPosition = CreateWorldPosition( translation.Global( i ) );

				if ( !onlyAvailable || IsAvailable( gridPosition ) )
					output.Add( gridPosition );
			}
		}

		public void GetGridPositions( Vector3 position, float radius, List<GridWorldPosition> output, bool onlyAvailable = false )
		{
			GetGridPositions( position, Convert.ToInt32( radius / NodeSize ), output, onlyAvailable ); 
		}

        public void UpdateCollisions( Vector3 position, int gridSize )
        {
            WaitForPhysicsUpdate = 5;
			GetGridPositions( position, gridSize, InternalCollisionBuffer );
		}

        public void ClearCollisions()
        {
			for ( int i = 0; i < InternalChunks.Length; i++ )
			{
				var chunk = InternalChunks[i];
				chunk.ClearCollisions();
			}
        }

		private void SetupSize( int numberOfChunks, int chunkGridSize, int nodeSize, int collisionSize )
		{
			var physicsBody = new PhysicsBody( Map.Physics );
			var collisionExtents = Vector3.One * collisionSize * 0.5f;
			var nodeExtents = Vector3.One * nodeSize * 0.5f;

			physicsBody.EnableSolidCollisions = false;

			var shape = physicsBody.AddBoxShape( Vector3.Zero, Rotation.Identity, collisionExtents );
			shape.AddTag( "pathfinder" );

			InternalNumberOfChunks = new GridDefinition( numberOfChunks, numberOfChunks );
			InternalCollisionExtents = collisionExtents;
			InternalCollisionSize = collisionSize;
			InternalNodeExtents = nodeExtents;
			InternalPhysicsBody = physicsBody;
			InternalChunkGridSize = new GridDefinition( chunkGridSize, chunkGridSize );
			InternalWorldGridSize = new GridDefinition(
				InternalChunkGridSize.Rows * InternalNumberOfChunks.Rows,
				InternalChunkGridSize.Columns * InternalNumberOfChunks.Columns
			);
			InternalNodeSize = nodeSize;
		}

		private PathRequest GetRequest()
		{
			var isValid = FlowFieldQueue.TryDequeue( out var flowField );

			if ( !isValid )
			{
				flowField = new FlowField( this );
			}

			return new PathRequest()
			{
				FlowField = flowField
			};
		}

		private void CreateChunks()
		{
			InternalChunks = new Chunk[NumberOfChunks.Size];

			for ( var i = 0; i < InternalNumberOfChunks.Size; i++ )
				InternalChunks[i] = new Chunk( i, InternalChunkGridSize );
		}

		private void ProcessBuffers()
        {
			for ( int i = 0; i < InternalCollisionBuffer.Count; i++ )
            {
				var collision = InternalCollisionBuffer[i];

				if ( collision.WorldIndex == int.MinValue )
					continue;

                var chunk = GetChunk( collision.ChunkIndex );

				if ( chunk.GetCollision( collision.NodeIndex ) == NodeCollision.Static )
				{
					// Static collisions never change, let's not update this node.
					continue;
				}

				if ( IsCollisionAt( GetPosition( collision ), collision.WorldIndex ) )
					chunk.SetCollision( collision.NodeIndex );
                else
                    chunk.RemoveCollision( collision.NodeIndex );

                if (InternalChunkBuffer.Contains( chunk ))
                    continue;

                InternalChunkBuffer.Add( chunk );
            }


			for ( int j = 0; j < InternalChunkBuffer.Count; j++ )
            {
				var i = InternalChunkBuffer[j];

				if ( GetChunk( i ) == null )
                    continue;

                ResetChunk( i );
            }

            if ( InternalChunkBuffer.Count > 0 )
			{
                PropagateWorldChange( InternalChunkBuffer );
			}

            InternalCollisionBuffer.Clear();
            InternalChunkBuffer.Clear();
        }

        private void PropagateWorldChange( List<int> chunks )
        {
			OnWorldChanged?.Invoke( this, chunks );
        }

        public Chunk GetChunk( int index )
        {
            return index >= InternalNumberOfChunks.Size || index < 0 ? null : InternalChunks[index];
        }

        private void ResetChunk( int i )
        {
			var chunk = GetChunk( i );

			CreatePortalsBetweenChunks( i, GridDirection.Up );
            CreatePortalsBetweenChunks( i, GridDirection.Right );
            CreatePortalsBetweenChunks( i, GridDirection.Left );
            CreatePortalsBetweenChunks( i, GridDirection.Down );

			chunk.ConnectGateways();

			var north = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Up, InternalNumberOfChunks ) );
			north?.ConnectGateways();

			var east = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Right, InternalNumberOfChunks ) );
			east?.ConnectGateways();

			var south = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Down, InternalNumberOfChunks ) );
			south?.ConnectGateways();

			var west = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Left, InternalNumberOfChunks ) );
			west?.ConnectGateways();
        }

        private void CreatePortalsBetweenChunks( int index, GridDirection direction )
        {
            var otherChunkIndex = GridUtility.GetNeighborIndex( index, direction, InternalNumberOfChunks );

            if ( !GridUtility.IsValid( otherChunkIndex ) )
                return;

            var thisChunk = GetChunk( index );
            var otherChunk = GetChunk( otherChunkIndex );

            var portalSize = 0;
            var range = GridUtility.GetBorderRange( InternalChunkGridSize, direction );

            thisChunk.ClearGateways(  direction );
            otherChunk.ClearGateways( direction.Opposite() );

            var thisGateway = new Gateway( thisChunk, direction );
            var otherGateway = new Gateway( otherChunk, direction.Opposite() );

			int portalIndex;

            for ( var x = range.MinX; x < range.MaxX; x++ )
            for ( var y = range.MinY; y < range.MaxY; y++ )
            {
                var thisNode = GridUtility.GetIndex( InternalChunkGridSize, y, x );
                var otherNode = GridUtility.GetMirrorIndex( InternalChunkGridSize, thisNode, direction );

                if ( thisChunk.IsImpassable( thisNode ) || otherChunk.IsImpassable( otherNode ) )
                {
                    if ( portalSize <= 0 )
                        continue;

                    if ( thisGateway.Nodes.Count > 0 )
                    {
                        thisChunk.AddGateway( thisGateway );
                        otherChunk.AddGateway( otherGateway );

						portalIndex =
                            CreateWorldPosition( thisGateway.Chunk, thisGateway.Median() ).WorldIndex
                            + CreateWorldPosition( otherGateway.Chunk, otherGateway.Median() ).WorldIndex;

                        Portals.Add( new Portal( portalIndex, thisGateway, otherGateway) );
                    }

                    thisGateway = new Gateway( thisChunk, direction );
                    otherGateway = new Gateway( otherChunk, direction.Opposite() );
                }
                else
                {
                    thisGateway.AddNode( thisNode );
                    otherGateway.AddNode( otherNode );
                }

                portalSize += 1;
            }

            if ( portalSize <= 0 ) return;

            if ( thisGateway.Nodes.Count == 0 )
                return;

            thisChunk.AddGateway( thisGateway );
            otherChunk.AddGateway( otherGateway );

            portalIndex =
                CreateWorldPosition( thisGateway.Chunk, thisGateway.Median() ).WorldIndex
                + CreateWorldPosition( otherGateway.Chunk, otherGateway.Median() ).WorldIndex;

            Portals.Add( new Portal( portalIndex, thisGateway, otherGateway ) );
        }

        public bool IsGateway( GridWorldPosition position )
        {
            return GetChunk( position.ChunkIndex ).HasGateway( position.NodeIndex );
        }

        public GridWorldPosition CreateWorldPosition( int worldIndex )
        {
            return new GridWorldPosition( worldIndex, GetChunkIndex( worldIndex ), GetNodeIndex( worldIndex ) );
        }

        public GridWorldPosition CreateWorldPosition( Vector3 position )
        {
			position += InternalPositionOffset;
			return CreateWorldPosition( GridUtility.GetIndex( WorldGridSize, MathUtility.FloorToInt( position.y / NodeSize ), MathUtility.FloorToInt( position.x / NodeSize ) ) );
        }

        public GridWorldPosition CreateWorldPosition( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( NumberOfChunks, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkGridSize, nodeIndex );

            var row = chunk.y * ChunkGridSize.Rows + node.y;
            var column = chunk.x * ChunkGridSize.Columns + node.x;

            return new GridWorldPosition(
                GridUtility.GetIndex( WorldGridSize, row, column ),
                chunkIndex,
                nodeIndex
            );
        }

        public Vector2i GetWorldCoordinates( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( NumberOfChunks, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkGridSize, nodeIndex );
            return new Vector2i( chunk.y * ChunkGridSize.Rows + node.y, chunk.x * ChunkGridSize.Columns + node.x );
        }

        public Vector3 GetPosition( Gateway gateway )
        {
			return GetPosition( gateway.Chunk, gateway.Median() );
		}

		public float GetHeight( int worldIndex )
		{
			return InternalHeightMap[Math.Clamp(worldIndex, 0, InternalHeightMap.Length - 1)];
		}

		public float GetHeight( GridWorldPosition worldPosition )
		{
			return GetHeight( worldPosition.WorldIndex );
		}

		public float GetHeight( Vector3 position )
		{
			return GetHeight( CreateWorldPosition( position ) );
		}

		public Vector3 GetPosition( int chunkIndex, int nodeIndex )
        {
            return GetLocalChunkPosition( chunkIndex ) + GetLocalNodePosition( nodeIndex ) - PositionOffset;
		}

		public Vector3 GetCenterPosition( Vector3 worldPosition )
		{
			return GetCenterPosition( CreateWorldPosition( worldPosition ) );
		}

		public Vector3 GetCenterPosition( GridWorldPosition worldPosition )
		{
			return GetPosition( worldPosition ) + InternalCenterOffset;
		}

        public Vector3 GetPosition( GridWorldPosition worldPosition )
        {
			return GetPosition( worldPosition.ChunkIndex, worldPosition.NodeIndex );
		}

        public Vector3 GetPosition( int index )
        {
			return GetPosition( CreateWorldPosition( index ) );
        }

        public Vector3 GetLocalChunkPosition( int chunkIndex )
        {
            return new Vector3( chunkIndex % NumberOfChunks.Columns * ChunkGridSize.Columns * NodeSize,
                (chunkIndex - chunkIndex % NumberOfChunks.Columns) / NumberOfChunks.Rows * ChunkGridSize.Columns * NodeSize, 0 );
        }

        public Vector3 GetLocalNodePosition( int nodeIndex )
        {
            return new Vector3( nodeIndex % ChunkGridSize.Columns * NodeSize, (nodeIndex - nodeIndex % ChunkGridSize.Columns) / ChunkGridSize.Columns * NodeSize, 0 );
        }

        public int GetChunkIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldGridSize, worldIndex );
            return GridUtility.GetIndex( NumberOfChunks,
                coordinates.y / ChunkGridSize.Rows,
                coordinates.x / ChunkGridSize.Columns );
        }

        public int GetNodeIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldGridSize, worldIndex );
            return GridUtility.GetIndex( ChunkGridSize,
                coordinates.y % ChunkGridSize.Rows,
                coordinates.x % ChunkGridSize.Columns );
        }
    }
}
