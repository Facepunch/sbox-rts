using System;
using System.Collections.Generic;
using Gamelib.Maths;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Sandbox;
using Gamelib.FlowFields.Entities;

namespace Gamelib.FlowFields
{
    public class Pathfinder
    {
		public static event Action<Pathfinder, List<int>> OnWorldChanged;

        public readonly List<Portal> Portals = new();

		private readonly List<GridWorldPosition> _collisionBuffer = new();
		private readonly List<int> _chunkBuffer = new();
		private readonly Queue<FlowField> _flowFields = new();

		[ServerVar( "ff_debug", Saved = true )]
		public static bool Debug { get; set; }

		private GridDefinition _numberOfChunks = new( 10, 10 );
        private GridDefinition _chunkGridSize = new( 10, 10 );
        private GridDefinition _worldGridSize = new( 100, 100 );
        private PhysicsBody _physicsBody;
        private Vector3 _positionOffset;
        private Vector3 _centerOffset;
        private Vector3 _collisionExtents;
        private Vector3 _nodeExtents;
		private float[] _heightMap;
        private Chunk[] _chunks;
        private int _waitForPhysicsUpdate = 3;
        private int _collisionSize = 100;
        private int _nodeSize = 50;

		public PhysicsBody PhysicsBody => _physicsBody;
		public Vector3 PositionOffset => _positionOffset;
		public Vector3 CenterOffset => _centerOffset;
		public Vector3 CollisionExtents => _collisionExtents;
		public Vector3 NodeExtents => _nodeExtents;
		public Vector3 Origin { get; private set; }

		public GridDefinition ChunkGridSize => _chunkGridSize;
		public GridDefinition NumberOfChunks => _numberOfChunks;
		public GridDefinition WorldGridSize => _worldGridSize;
		public Chunk[] Chunks => _chunks;
		public int CollisionSize => _collisionSize;
		public int NodeSize => _nodeSize;

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
            if (_waitForPhysicsUpdate > 0)
            {
                _waitForPhysicsUpdate--;
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
			_flowFields.Enqueue( request.FlowField );
			request.FlowField = null;
		}

		public void UpdateCollisions()
        {
            for ( var index = 0; index < WorldGridSize.Size; index++ )
            {
				UpdateCollisions( index );
			}
		}

		public TraceResult GetCollisionTrace( Vector3 position, int worldIndex )
		{
			var transform = _physicsBody.Transform;
			var heightMap = _heightMap[worldIndex];

			transform.Position = (position + _centerOffset).WithZ( _collisionExtents.z + heightMap + 5f );

			var trace = Trace.Sweep( _physicsBody, transform, transform )
				.WithoutTags( "ff_ground" )
				.WithoutTags( "ff_ignore" )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
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

			var transform = _physicsBody.Transform;
			transform.Position = (position + _centerOffset).WithZ( _nodeExtents.z + _heightMap[worldIndex] );
			DebugOverlay.Box( duration, transform.Position, -_collisionExtents, _collisionExtents, Color.Red, false );
			DebugOverlay.Box( duration, transform.Position, -_nodeExtents, _nodeExtents, color.HasValue ? color.Value : Color.White, false );
		}

		public void DrawBox( int worldIndex, Color? color = null, float duration = 1f )
		{
			DrawBox( GetPosition( worldIndex ), worldIndex, color, duration );
		}

		public void DrawBox( GridWorldPosition position, Color? color = null, float duration = 1f )
		{
			DrawBox( GetPosition( position ), position.WorldIndex, color, duration );
		}

        public void Initialize()
        {
			_positionOffset = Origin + new Vector3(
				_worldGridSize.Columns * _nodeSize / 2f,
				_worldGridSize.Rows * _nodeSize / 2f,
				0f
			);

			_centerOffset = new Vector3(
				NodeSize / 2f,
				NodeSize / 2f,
				0f
			);

			_chunkBuffer.Clear();

			CreateChunks();
			CreateHeightMap();
			UpdateCollisions();
			ConnectPortals();

			// Create a good amount of flow fields ready in the pool.
			for ( var i = 0; i < 10; i++ )
			{
				_flowFields.Enqueue( new FlowField( this ) );
			}
		}

		public void CreateHeightMap()
		{
			var worldSizeLength = _worldGridSize.Size;

			_heightMap = new float[worldSizeLength];

			var groundHeight = FlowFieldGround.Bounds.Maxs.z;

			for ( var index = 0; index < worldSizeLength; index++ )
			{
				/*
				var position = GetPosition( index );
				var trace = Trace.Ray( position.WithZ( 1000f ), position )
					.EntitiesOnly()
					.WithTag( "ff_ground" )
					.Run();

				if ( trace.Hit )
					_heightMap[index] = trace.EndPos.z;
				else
					_heightMap[index] = position.z;
				*/

				// TODO: The ground must be flat for now.
				_heightMap[index] = groundHeight;
			}
		}

		public void ConnectPortals()
        {
            Portals.Clear();

            for ( var i = 0; i < _numberOfChunks.Size; i++ )
			{
				var chunk = GetChunk( i );
				chunk.ClearGateways();
			}

            for ( var i = 0; i < _numberOfChunks.Size; i++ )
            {
                CreatePortalsBetweenChunks( i, GridDirection.Up );
                CreatePortalsBetweenChunks( i, GridDirection.Right );
            }

			for ( int i = 0; i < _chunks.Length; i++ )
			{
				var chunk = _chunks[i];
				chunk.ConnectGateways();
			}
        }

        public void ClearChunks()
        {
            _chunks = null;
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

            return GetChunk(chunk).GetCost( node );
        }

		public void UpdateCollisions( Vector3 position, float radius )
		{
			UpdateCollisions( position, Convert.ToInt32( radius / NodeSize ) );
		}

		public void GetGridPositions( Vector3 position, int gridSize, List<Vector3> output )
		{
			var grid = new GridDefinition( gridSize * 2 + 1, gridSize * 2 + 1 );
			var worldPivotPosition = CreateWorldPosition( position );
			var translation = new GridConverter( grid, WorldGridSize, grid.Size / 2, worldPivotPosition.WorldIndex );

			for ( var i = 0; i < grid.Size; i++ )
			{
				output.Add( GetPosition( translation.Global( i ) ) );
			}
		}

		public void GetGridPositions( Vector3 position, float radius, List<Vector3> output )
		{
			GetGridPositions( position, Convert.ToInt32( radius / NodeSize ), output );
		}

		public void GetGridPositions( Vector3 position, int gridSize, List<GridWorldPosition> output )
		{
			var grid = new GridDefinition( gridSize * 2 + 1, gridSize * 2 + 1 );
			var worldPivotPosition = CreateWorldPosition( position );
			var translation = new GridConverter( grid, WorldGridSize, grid.Size / 2, worldPivotPosition.WorldIndex );

			for ( var i = 0; i < grid.Size; i++ )
			{
				output.Add( CreateWorldPosition( translation.Global( i ) ) );
			}
		}

		public void GetGridPositions( Vector3 position, float radius, List<GridWorldPosition> output )
		{
			GetGridPositions( position, Convert.ToInt32( radius / NodeSize ), output ); 
		}

        public void UpdateCollisions( Vector3 position, int gridSize )
        {
            _waitForPhysicsUpdate = 5;
			GetGridPositions( position, gridSize, _collisionBuffer );
		}

        public void ClearCollisions()
        {
			for ( int i = 0; i < _chunks.Length; i++ )
			{
				var chunk = _chunks[i];
				chunk.ClearCollisions();
			}
        }

		private void SetupSize( int numberOfChunks, int chunkGridSize, int nodeSize, int collisionSIze )
		{
			var physicsBody = PhysicsWorld.AddBody();
			var collisionExtents = Vector3.One * collisionSIze * 0.5f;
			var nodeExtents = Vector3.One * nodeSize * 0.5f;

			physicsBody.CollisionEnabled = false;
			physicsBody.AddBoxShape( Vector3.Zero, Rotation.Identity, collisionExtents );

			_numberOfChunks = new GridDefinition( numberOfChunks, numberOfChunks );
			_collisionExtents = collisionExtents;
			_nodeExtents = nodeExtents;
			_physicsBody = physicsBody;
			_chunkGridSize = new GridDefinition( chunkGridSize, chunkGridSize );
			_worldGridSize = new GridDefinition(
				_chunkGridSize.Rows * _numberOfChunks.Rows,
				_chunkGridSize.Columns * _numberOfChunks.Columns
			);
			_nodeSize = nodeSize;
		}

		private PathRequest GetRequest()
		{
			var isValid = _flowFields.TryDequeue( out var flowField );

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
			_chunks = new Chunk[NumberOfChunks.Size];

			for ( var i = 0; i < _numberOfChunks.Size; i++ )
				_chunks[i] = new Chunk( i, _chunkGridSize );
		}

		private void ProcessBuffers()
        {
			for ( int i = 0; i < _collisionBuffer.Count; i++ )
            {
				var collision = _collisionBuffer[i];

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

                if (_chunkBuffer.Contains( chunk ))
                    continue;

                _chunkBuffer.Add( chunk );
            }


			for ( int j = 0; j < _chunkBuffer.Count; j++ )
            {
				var i = _chunkBuffer[j];

				if ( GetChunk( i ) == null )
                    continue;

                ResetChunk( i );
            }

            if ( _chunkBuffer.Count > 0 )
			{
                PropagateWorldChange( _chunkBuffer );
			}

            _collisionBuffer.Clear();
            _chunkBuffer.Clear();
        }

        private void PropagateWorldChange( List<int> chunks )
        {
			OnWorldChanged?.Invoke( this, chunks );
        }

        public Chunk GetChunk( int index )
        {
            return index >= _numberOfChunks.Size || index < 0 ? null : _chunks[index];
        }

        private void ResetChunk( int i )
        {
			var chunk = GetChunk( i );

			CreatePortalsBetweenChunks( i, GridDirection.Up );
            CreatePortalsBetweenChunks( i, GridDirection.Right );
            CreatePortalsBetweenChunks( i, GridDirection.Left );
            CreatePortalsBetweenChunks( i, GridDirection.Down );

			chunk.ConnectGateways();

			var north = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Up, _numberOfChunks ) );
			north?.ConnectGateways();

			var east = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Right, _numberOfChunks ) );
			east?.ConnectGateways();

			var south = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Down, _numberOfChunks ) );
			south?.ConnectGateways();

			var west = GetChunk( GridUtility.GetNeighborIndex( i, GridDirection.Left, _numberOfChunks ) );
			west?.ConnectGateways();
        }

        private void CreatePortalsBetweenChunks( int index, GridDirection direction )
        {
            var otherChunkIndex = GridUtility.GetNeighborIndex( index, direction, _numberOfChunks );

            if ( !GridUtility.IsValid( otherChunkIndex ) )
                return;

            var thisChunk = GetChunk( index );
            var otherChunk = GetChunk( otherChunkIndex );

            var portalSize = 0;
            var range = GridUtility.GetBorderRange( _chunkGridSize, direction );

            thisChunk.ClearGateways(  direction );
            otherChunk.ClearGateways( direction.Opposite() );

            var thisGateway = new Gateway( thisChunk, direction );
            var otherGateway = new Gateway( otherChunk, direction.Opposite() );

			int portalIndex;

            for ( var x = range.MinX; x < range.MaxX; x++ )
            for ( var y = range.MinY; y < range.MaxY; y++ )
            {
                var thisNode = GridUtility.GetIndex( _chunkGridSize, y, x );
                var otherNode = GridUtility.GetMirrorIndex( _chunkGridSize, thisNode, direction );

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
			position += _positionOffset;
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
			return _heightMap[Math.Clamp(worldIndex, 0, _heightMap.Length - 1)];
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
			return GetPosition( worldPosition ) + _centerOffset;
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
