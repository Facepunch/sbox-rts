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
        public float Scale = 1f;

		private readonly List<GridWorldPosition> _collisionBuffer = new();
		private readonly List<int> _chunkBuffer = new();

		private GridDefinition _worldChunkSize = new(10, 10);
        private GridDefinition _chunkSize = new(10, 10);
        private GridDefinition _worldNodeSize = new(100, 100);
        private Chunk[] _chunks;
        private Vector3 _centerOffset;
        private int _waitForPhysicsUpdate = 3;

        public GridDefinition ChunkSize
        {
            get => _chunkSize;
            set => _chunkSize = value;
        }

        public GridDefinition WorldChunkSize
        {
            get => _worldChunkSize;
            set => _worldChunkSize = value;
        }

        public GridDefinition WorldNodeSize
        {
            get
            {
                if (_worldNodeSize == null || _worldNodeSize.Columns == 0)
				{
					_worldNodeSize = new GridDefinition(
						_chunkSize.Rows * _worldChunkSize.Rows,
						_chunkSize.Columns * _worldChunkSize.Columns
					);
				}

                return _worldNodeSize;
            }
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
            CalculateCenterOffset();

            for ( var index = 0; index < WorldNodeSize.Size; index++ )
            {
                var position = GetPosition( index ) + _centerOffset;

				if ( Physics.GetEntitiesInSphere( position, Scale / 2 ).Count() > 0 )
					GetChunk( GetChunkIndex( index ) ).SetCollision( GetNodeIndex( index ) );
            }
        }

        public void Init()
        {
            if ( _chunks == null || !_chunks.Any() )
                CreateChunks();

            ClearCollisions();
            _chunkBuffer.Clear();
            ConnectPortals();
        }

        public void ConnectPortals()
        {
            Portals.Clear();

            for ( var i = 0; i < _worldChunkSize.Size; i++ )
                GetChunk(i).ClearGateways();

            for ( var i = 0; i < _worldChunkSize.Size; i++ )
            {
                CreatePortalsBetweenChunks( i, GridDirection.Up );
                CreatePortalsBetweenChunks( i, GridDirection.Right );
            }

            foreach (var chunk in _chunks)
                chunk.ConnectGateways();
        }

        public void ClearChunks()
        {
            _chunks = null;
        }

		private void CalculateCenterOffset()
		{
			_centerOffset = new Vector3( Scale / 2f, Scale / 2f, 0 );
		}

		private void CreateChunks()
        {
            _chunks = new Chunk[WorldChunkSize.Size];

            for (var i = 0; i < _worldChunkSize.Size; i++)
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


        public void UpdateArea( Vector3 position, int size )
        {
            _waitForPhysicsUpdate = 5;

            var worldPivotPosition = CreateWorldPosition( position );
            var grid = new GridDefinition( size * 2 + 1, size * 2 + 1 );
            var translation = new GridConverter( grid, WorldNodeSize, grid.Size / 2, worldPivotPosition.WorldIndex );

            for ( var i = 0; i < grid.Size; i++ )
            {
                var worldPosition = CreateWorldPosition( translation.Global(i) );
                _collisionBuffer.Add( worldPosition );
            }
        }

        public void ClearCollisions()
        {
            foreach ( var chunk in _chunks )
				chunk.ClearCollisions();
        }

        private void ProcessBuffers()
        {
            foreach ( var collision in _collisionBuffer )
            {
                if ( collision.WorldIndex == int.MinValue ) continue;

                var chunk = GetChunk( collision.ChunkIndex );
                var position = GetPositionCentered( collision );

                if ( Physics.GetEntitiesInSphere( position, Scale / 2 ).Count() > 0 )
                    chunk.SetCollision( collision.NodeIndex );
                else
                    chunk.RemoveCollision( collision.NodeIndex );


                if (_chunkBuffer.Contains( chunk ))
                    continue;

                _chunkBuffer.Add( chunk );
            }


            foreach ( var i in _chunkBuffer )
            {
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
            return index >= _worldChunkSize.Size || index < 0 ? null : _chunks[index];
        }

        private void ResetChunk( int i )
        {
            CreatePortalsBetweenChunks(i, GridDirection.Up);
            CreatePortalsBetweenChunks(i, GridDirection.Right);
            CreatePortalsBetweenChunks(i, GridDirection.Left);
            CreatePortalsBetweenChunks(i, GridDirection.Down);
            GetChunk(i).ConnectGateways();

            GetChunk(GridUtility.GetNeighborIndex(i, GridDirection.Up, _worldChunkSize))?.ConnectGateways();
            GetChunk(GridUtility.GetNeighborIndex(i, GridDirection.Right, _worldChunkSize))?.ConnectGateways();
            GetChunk(GridUtility.GetNeighborIndex(i, GridDirection.Down, _worldChunkSize))?.ConnectGateways();
            GetChunk(GridUtility.GetNeighborIndex(i, GridDirection.Left, _worldChunkSize))?.ConnectGateways();
        }

        private void CreatePortalsBetweenChunks( int index, GridDirection direction )
        {
            var otherChunkIndex = GridUtility.GetNeighborIndex( index, direction, WorldChunkSize );

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

            for ( var x = range.XMin; x < range.XMax; x++ )
            for ( var y = range.YMin; y < range.YMax; y++ )
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
            return CreateWorldPosition( GridUtility.GetIndex( WorldNodeSize, MathUtility.FloorToInt( position.y / Scale ), MathUtility.FloorToInt( position.x / Scale ) ) );
        }

        public GridWorldPosition CreateWorldPosition( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( WorldChunkSize, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkSize, nodeIndex );

            var row = chunk.y * ChunkSize.Rows + node.y;
            var column = chunk.x * ChunkSize.Columns + node.x;

            return new GridWorldPosition(
                GridUtility.GetIndex( WorldNodeSize, row, column ),
                chunkIndex,
                nodeIndex
            );
        }

        public Vector2i GetWorldCoordinates( int chunkIndex, int nodeIndex )
        {
            var chunk = GridUtility.GetCoordinates( WorldChunkSize, chunkIndex );
            var node = GridUtility.GetCoordinates( ChunkSize, nodeIndex );
            return new Vector2i( chunk.y * ChunkSize.Rows + node.y, chunk.x * ChunkSize.Columns + node.x );
        }

        public Vector3 GetPosition( Gateway gateway )
        {
            return GetPosition( gateway.Chunk, gateway.Median() );
        }

        public Vector3 GetPosition( int chunkIndex, int nodeIndex )
        {
            return GetChunkPosition( chunkIndex ) + GetNodePosition( nodeIndex );
        }

        public Vector3 GetPosition( GridWorldPosition worldPosition )
        {
            return GetPosition( worldPosition.ChunkIndex, worldPosition.NodeIndex );
        }

        public Vector3 GetPositionCentered( GridWorldPosition worldPosition )
        {
            return GetPosition( worldPosition ) + _centerOffset;
        }

        public Vector3 GetPosition( int index )
        {
            return GetPosition( CreateWorldPosition( index ) );
        }

        public Vector3 GetChunkPosition( int chunkIndex )
        {
            return new Vector3( chunkIndex % WorldChunkSize.Columns * ChunkSize.Columns * Scale,
                (chunkIndex - chunkIndex % WorldChunkSize.Columns) / WorldChunkSize.Rows * ChunkSize.Columns * Scale, 0 );
        }

        public Vector3 GetNodePosition( int nodeIndex )
        {
            return new Vector3( nodeIndex % ChunkSize.Columns * Scale, (nodeIndex - nodeIndex % ChunkSize.Columns) / ChunkSize.Columns * Scale, 0 );
        }

        public int GetChunkIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldNodeSize, worldIndex );
            return GridUtility.GetIndex( WorldChunkSize,
                coordinates.y / ChunkSize.Rows,
                coordinates.x / ChunkSize.Columns );
        }

        public int GetNodeIndex( int worldIndex )
        {
            var coordinates = GridUtility.GetCoordinates( WorldNodeSize, worldIndex );
            return GridUtility.GetIndex( ChunkSize,
                coordinates.y % ChunkSize.Rows,
                coordinates.x % ChunkSize.Columns );
        }
    }
}
