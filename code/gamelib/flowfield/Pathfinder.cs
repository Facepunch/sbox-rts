using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Maths;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Sandbox;

namespace Gamelib.FlowFields
{
    public class Pathfinder
    {
		public static event Action<Pathfinder, List<int>> OnWorldChanged;

        public readonly List<Portal> Portals = new();

		private readonly List<GridWorldPosition> _collisionBuffer = new();
		private readonly List<int> _chunkBuffer = new();

		private GridDefinition _numberOfChunks = new(10, 10);
        private GridDefinition _chunkSize = new(10, 10);
        private GridDefinition _worldSize = new(100, 100);
        private PhysicsBody _physicsBody;
        private Vector3 _halfExtents;
        private Chunk[] _chunks;
        private int _waitForPhysicsUpdate = 3;
        private float _scale = 1f;

		public PhysicsBody PhysicsBody => _physicsBody;
		public Vector3 PositionOffset { get; private set; }
		public Vector3 CenterOffset { get; private set; }

        public GridDefinition ChunkSize
        {
            get => _chunkSize;
            set => _chunkSize = value;
        }

        public GridDefinition NumberOfChunks
        {
            get => _numberOfChunks;
            set => _numberOfChunks = value;
        }

		public float Scale => _scale;

		public GridDefinition WorldSize
        {
            get
            {
                if (_worldSize == null || _worldSize.Columns == 0)
				{
					_worldSize = new GridDefinition(
						_chunkSize.Rows * _numberOfChunks.Rows,
						_chunkSize.Columns * _numberOfChunks.Columns
					);
				}

                return _worldSize;
            }
        }

		public Pathfinder( int numberOfChunks, int chunkSize, float scale = 1f )
		{
			var physicsBody = PhysicsWorld.AddBody();
			var halfExtents = Vector3.One * scale * 0.5f;

			physicsBody.CollisionEnabled = false;
			physicsBody.AddBoxShape( Vector3.Zero, Rotation.Identity, halfExtents );

			_numberOfChunks = new GridDefinition( numberOfChunks, numberOfChunks );
			_physicsBody = physicsBody;
			_halfExtents = halfExtents;
			_chunkSize = new GridDefinition( chunkSize, chunkSize );
			_worldSize = new GridDefinition(
				_chunkSize.Rows * _numberOfChunks.Rows,
				_chunkSize.Columns * _numberOfChunks.Columns
			);
			_scale = scale;
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

        public void UpdateCollisions()
        {
            for ( var index = 0; index < WorldSize.Size; index++ )
            {
				UpdateCollisions( index );
            }
		}

		public bool IsCollisionAt( Vector3 position )
		{
			var transform = _physicsBody.Transform;
			transform.Position = (position + CenterOffset).WithZ( _halfExtents.z * 2.5f );
			return Trace.Sweep( _physicsBody, transform, transform ).Run().Hit;
		}

		public bool IsAvailable( Vector3 position )
		{
			var worldPosition = CreateWorldPosition( position );
			return !GetChunk( worldPosition.ChunkIndex ).IsImpassable( worldPosition.NodeIndex );
		}

		public void UpdateCollisions( int worldIndex )
		{
			var chunk = GetChunk( GetChunkIndex( worldIndex ) );
			var nodeIndex = GetNodeIndex( worldIndex );

			if ( IsCollisionAt( GetPosition( worldIndex ) ) )
				chunk.SetCollision( nodeIndex );
			else
				chunk.RemoveCollision( nodeIndex );
		}

        public void Initialize()
        {
			PositionOffset = new Vector3(
				_worldSize.Columns * _scale / 2f,
				_worldSize.Rows * _scale / 2f
			);

			CenterOffset = new Vector3(
				Scale / 2f,
				Scale / 2f
			);

			if ( _chunks == null || !_chunks.Any() )
                CreateChunks();

            ClearCollisions();
            _chunkBuffer.Clear();
            ConnectPortals();
        }

        public void ConnectPortals()
        {
            Portals.Clear();

            for ( var i = 0; i < _numberOfChunks.Size; i++ )
                GetChunk(i).ClearGateways();

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

		private void CreateChunks()
        {
            _chunks = new Chunk[NumberOfChunks.Size];

            for (var i = 0; i < _numberOfChunks.Size; i++)
				_chunks[i] = new Chunk( i, _chunkSize );
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
			UpdateCollisions( position, Convert.ToInt32( radius / Scale ) );
		}

		public void GetGridPositions( Vector3 position, int gridSize, List<GridWorldPosition> output )
		{
			var grid = new GridDefinition( gridSize * 2 + 1, gridSize * 2 + 1 );
			var worldPivotPosition = CreateWorldPosition( position );
			var translation = new GridConverter( grid, WorldSize, grid.Size / 2, worldPivotPosition.WorldIndex );

			for ( var i = 0; i < grid.Size; i++ )
			{
				output.Add( CreateWorldPosition( translation.Global( i ) ) );
			}
		}

		public void GetGridPositions( Vector3 position, float radius, List<GridWorldPosition> output )
		{
			GetGridPositions( position, Convert.ToInt32( radius / Scale ), output ); 
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

        private void ProcessBuffers()
        {
			for ( int i = 0; i < _collisionBuffer.Count; i++ )
            {
				var collision = _collisionBuffer[i];
				if ( collision.WorldIndex == int.MinValue ) continue;

                var chunk = GetChunk( collision.ChunkIndex );

				if ( IsCollisionAt( GetPosition( collision ) ) )
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

                ResetChunk(i);
            }

            if ( _chunkBuffer.Any() )
                PropagateWorldChange( _chunkBuffer );

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
            CreatePortalsBetweenChunks( i, GridDirection.Up );
            CreatePortalsBetweenChunks( i, GridDirection.Right );
            CreatePortalsBetweenChunks( i, GridDirection.Left );
            CreatePortalsBetweenChunks( i, GridDirection.Down );
            GetChunk( i ).ConnectGateways();

            GetChunk( GridUtility.GetNeighborIndex(i, GridDirection.Up, _numberOfChunks) )?.ConnectGateways();
            GetChunk( GridUtility.GetNeighborIndex(i, GridDirection.Right, _numberOfChunks) )?.ConnectGateways();
            GetChunk( GridUtility.GetNeighborIndex(i, GridDirection.Down, _numberOfChunks) )?.ConnectGateways();
            GetChunk( GridUtility.GetNeighborIndex(i, GridDirection.Left, _numberOfChunks) )?.ConnectGateways();
        }

        private void CreatePortalsBetweenChunks( int index, GridDirection direction )
        {
            var otherChunkIndex = GridUtility.GetNeighborIndex( index, direction, NumberOfChunks );

            if ( !GridUtility.IsValid( otherChunkIndex ) )
                return;

            var thisChunk = GetChunk( index );
            var otherChunk = GetChunk( otherChunkIndex );

            var portalSize = 0;
            var range = GridUtility.GetBorderRange(ChunkSize, direction);

            thisChunk.ClearGateways(  direction );
            otherChunk.ClearGateways(direction.Opposite() );

            var thisGateway = new Gateway( thisChunk, direction );
            var otherGateway = new Gateway( otherChunk, direction.Opposite() );

            for ( var x = range.MinX; x < range.MaxX; x++ )
            for ( var y = range.MinY; y < range.MaxY; y++ )
            {
                var thisNode = GridUtility.GetIndex( ChunkSize, y, x );
                var otherNode = GridUtility.GetMirrorIndex( ChunkSize, thisNode, direction );

                if ( thisChunk.IsImpassable(thisNode) || otherChunk.IsImpassable(otherNode) )
                {
                    if ( portalSize <= 0 )
                        continue;

                    if ( thisGateway.Nodes.Any() )
                    {
                        thisChunk.AddGateway( thisGateway );
                        otherChunk.AddGateway( otherGateway );

                        var portalIndex =
                            CreateWorldPosition( thisGateway.Chunk, thisGateway.Median() ).WorldIndex
                            + CreateWorldPosition( otherGateway.Chunk, otherGateway.Median() ).WorldIndex;

                        Portals.Add( new Portal( portalIndex, thisGateway, otherGateway) );
                    }

                    thisGateway = new Gateway( thisChunk, direction);
                    otherGateway = new Gateway( otherChunk, direction.Opposite() );
                }
                else
                {
                    thisGateway.AddNode(thisNode);
                    otherGateway.AddNode(otherNode);
                }

                portalSize += 1;
            }

            if (portalSize <= 0) return;

            {
                if ( !thisGateway.Nodes.Any() )
                    return;

                thisChunk.AddGateway( thisGateway );
                otherChunk.AddGateway( otherGateway );

                var portalIndex =
                    CreateWorldPosition( thisGateway.Chunk, thisGateway.Median() ).WorldIndex
                    + CreateWorldPosition( otherGateway.Chunk, otherGateway.Median() ).WorldIndex;

                Portals.Add( new Portal( portalIndex, thisGateway, otherGateway ) );
            }
        }

        public bool IsGateway( GridWorldPosition position )
        {
            return GetChunk( position.ChunkIndex ).HasGateway( position.NodeIndex );
        }

        public GridWorldPosition CreateWorldPosition( int worldIndex )
        {
            return new GridWorldPosition( worldIndex, GetChunkIndex(worldIndex), GetNodeIndex(worldIndex) );
        }

        public GridWorldPosition CreateWorldPosition( Vector3 position )
        {
			position += PositionOffset;
			return CreateWorldPosition( GridUtility.GetIndex( WorldSize, MathUtility.FloorToInt( position.y / Scale ), MathUtility.FloorToInt( position.x / Scale ) ) );
        }

        public GridWorldPosition CreateWorldPosition( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( NumberOfChunks, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkSize, nodeIndex );

            var row = chunk.y * ChunkSize.Rows + node.y;
            var column = chunk.x * ChunkSize.Columns + node.x;

            return new GridWorldPosition(
                GridUtility.GetIndex( WorldSize, row, column ),
                chunkIndex,
                nodeIndex
            );
        }

        public Vector2i GetWorldCoordinates( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( NumberOfChunks, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkSize, nodeIndex );
            return new Vector2i( chunk.y * ChunkSize.Rows + node.y, chunk.x * ChunkSize.Columns + node.x );
        }

        public Vector3 GetPosition( Gateway gateway )
        {
			return GetPosition( gateway.Chunk, gateway.Median() );
		}

        public Vector3 GetPosition( int chunkIndex, int nodeIndex )
        {
            return GetLocalChunkPosition( chunkIndex ) + GetLocalNodePosition( nodeIndex ) - PositionOffset;
		}

		public Vector3 GetCenterPosition( GridWorldPosition worldPosition )
		{
			return GetPosition( worldPosition ) + CenterOffset;
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
            return new Vector3( chunkIndex % NumberOfChunks.Columns * ChunkSize.Columns * Scale,
                (chunkIndex - chunkIndex % NumberOfChunks.Columns) / NumberOfChunks.Rows * ChunkSize.Columns * Scale, 0 );
        }

        public Vector3 GetLocalNodePosition( int nodeIndex )
        {
            return new Vector3( nodeIndex % ChunkSize.Columns * Scale, (nodeIndex - nodeIndex % ChunkSize.Columns) / ChunkSize.Columns * Scale, 0 );
        }

        public int GetChunkIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldSize, worldIndex );
            return GridUtility.GetIndex( NumberOfChunks,
                coordinates.y / ChunkSize.Rows,
                coordinates.x / ChunkSize.Columns );
        }

        public int GetNodeIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldSize, worldIndex );
            return GridUtility.GetIndex( ChunkSize,
                coordinates.y % ChunkSize.Rows,
                coordinates.x % ChunkSize.Columns );
        }
    }
}
