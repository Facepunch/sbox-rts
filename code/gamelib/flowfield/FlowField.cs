using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Algorithms;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Sandbox;

namespace Gamelib.FlowFields
{
    public class GatewaySubPath
    {
        public Gateway FromConnectionGateway;
        public Gateway UntilConnectionGateway;
    }

    public interface IIntegrationContainer
    {
        bool HasIntegration(int index);
        Integration GetIntegration(int index);
        Pathfinder GetPathfinder();
    }

    public class FlowField : IIntegrationContainer
    {
        public delegate void Changed();

		public readonly Dictionary<int, List<Gateway>> DestinationGateways = new();
		public readonly Dictionary<Gateway, Gateway> GatewayPath = new();
		public readonly Dictionary<int, Integration> Integrations = new();

		protected readonly Dictionary<int, int[]> Flows = new();
        protected readonly Stack<Gateway> IntegrationStack = new();

        private readonly Queue<int> _flowQueue = new();
        private readonly List<int> _flowsToBeProcessed = new();
        private readonly Dictionary<int, int[]> _previousFlows = new();
        private readonly Dictionary<Gateway, Gateway> _previousGatewayPath = new();

        private Vector3 _destinationPosition;

        public Changed ChangedDelegates;

        public List<int> DestinationIndexes = new();
        public int DestinationIndex;

        public FlowField( Pathfinder pathfinder )
        {
            Pathfinder = pathfinder;
			Pathfinder.OnWorldChanged += OnPathfinderWorldChanged;
        }

        public Pathfinder Pathfinder { get; }

        public virtual Vector3 DestinationPosition => _destinationPosition;

        public bool HasIntegration(int index)
        {
            return Integrations.ContainsKey(index);
        }

        public Integration GetIntegration(int index)
        {
            if (!Integrations.ContainsKey(index)) Integrations[index] = IntegrationService.CreateIntegration(Pathfinder.ChunkSize);

            return Integrations[index];
        }

        public Pathfinder GetPathfinder()
        {
            return Pathfinder;
        }

        public void OnPathfinderWorldChanged( Pathfinder pathfinder, List<int> chunks )
        {
            if ( Pathfinder != pathfinder ) return;

            if ( Integrations.Any( integration => chunks.Contains( integration.Key ) ) )
                UpdatePaths( chunks );
        }

        public Dictionary<int, Integration> GetIntegrations()
        {
            return Integrations;
        }

        public void CreateDestinationGateways(List<int> worldIndexes)
        {
            if ( worldIndexes.Count == 0 )
                return;

            DestinationIndex = worldIndexes.First();
            DestinationIndexes.Clear();
            DestinationIndexes.AddRange(worldIndexes);
            DestinationGateways.Clear();

            _destinationPosition = Pathfinder.GetCenterPosition(Pathfinder.CreateWorldPosition(DestinationIndex));

            var chunks = new Dictionary<int, List<int>>();

            foreach ( var worldIndex in worldIndexes )
            {
                var chunkIndex = Pathfinder.GetChunkIndex(worldIndex);

                if (!chunks.ContainsKey(chunkIndex)) chunks[chunkIndex] = new();

                chunks[chunkIndex].Add(Pathfinder.GetNodeIndex(worldIndex));
            }


            foreach ( var data in chunks )
            {
                var chunk = Pathfinder.GetChunk( data.Key );

                foreach ( var gateway in chunk.GetGateways() )
                {
                    if ( !chunk.Connects(gateway, data.Value) ) continue;

                    if ( !DestinationGateways.ContainsKey(data.Key) )
						DestinationGateways[data.Key] = new();

                    DestinationGateways[data.Key].Add(gateway);
                }
            }
        }

        public void ResetDestination()
        {
            DestinationGateways.Clear();
        }

        public void UpdatePaths( List<int> changedChunks )
        {
            _previousGatewayPath.Clear();
            _previousFlows.Clear();

            CreateDestinationGateways( DestinationIndexes.ToList() );

            Integrations.Clear();
            GatewayPath.Clear();
            Flows.Clear();
        }

        public void UpdatePaths()
        {
            _previousGatewayPath.Clear();
            _previousFlows.Clear();
            CreateDestinationGateways( DestinationIndexes.ToList() );

            Integrations.Clear();
            GatewayPath.Clear();
            Flows.Clear();
        }

        protected virtual void SeekPath( Vector3 startPosition )
        {
            var subPath = AStarPortal.Default.CalculatePath( this, startPosition );
            var worldPosition = Pathfinder.CreateWorldPosition( startPosition );

            if ( DestinationGateways.ContainsKey( worldPosition.ChunkIndex ) )
            {
                var destinationGateway = DestinationGateways[worldPosition.ChunkIndex];

                IntegrationStack.Push(destinationGateway.First());
                _flowQueue.Enqueue(destinationGateway.First().Chunk);

                IntegrateStack();
            }

            if ( subPath != null )
            {
                BuildIntegrationStack(subPath.FromConnectionGateway, subPath.UntilConnectionGateway);
                IntegrateStack();
            }

            QueueFlow();

            ChangedDelegates?.Invoke();
        }

        public bool IsAvailable( GridWorldPosition worldPosition )
        {
            return IsAvailable( worldPosition.ChunkIndex, worldPosition.NodeIndex );
        }

        public bool IsAvailable( int chunk, int index )
        {
            return !Pathfinder.GetChunk(chunk).IsImpassable(index);
        }

        private void BuildIntegrationStack( Gateway fromGateway, Gateway untilGateway )
        {
            while ( !Equals( fromGateway, untilGateway ) )
            {
                if ( RestoreFromCache( fromGateway ) )
                {
                    fromGateway = GatewayPath[fromGateway];
                    continue;
                }

                _flowQueue.Enqueue(fromGateway.Chunk);
                IntegrationStack.Push(fromGateway);

                DebugOverlay.Line( Pathfinder.GetPosition(fromGateway), Pathfinder.GetPosition( GatewayPath[fromGateway] ), Color.Cyan, 5f );
                fromGateway = GatewayPath[fromGateway];
            }

            if ( !RestoreFromCache( untilGateway ) )
                IntegrationStack.Push( untilGateway );

            if ( !RestoreFromCache( fromGateway ) )
                _flowQueue.Enqueue(fromGateway.Chunk);

            if ( !RestoreFromCache( untilGateway ) )
                _flowQueue.Enqueue( untilGateway.Chunk );
        }

        private bool RestoreFromCache( Gateway connectionGateway )
        {
            if ( !_previousGatewayPath.ContainsKey( connectionGateway ) || !GatewayPath.ContainsKey( connectionGateway ) )
                return false;

            if ( !_previousGatewayPath[connectionGateway].Equals( GatewayPath[connectionGateway] ) )
                return false;

            Flows[connectionGateway.Chunk] = _previousFlows[connectionGateway.Chunk];
            return true;
        }

        private void IntegrateStack()
        {
            foreach ( var index in DestinationIndexes )
            {
                var integration = GetIntegration( Pathfinder.GetChunkIndex(index) );
                var node = Pathfinder.GetNodeIndex( index );

                integration.SetValue(node, 1);
                integration.Enqueue(node);
            }


            while ( IntegrationStack.Count > 0 )
            {
                var gateway = IntegrationStack.Pop();
                var integration = GetIntegration(gateway.Chunk);

                IntegrationsPathService.Default.Integrate( this, integration, gateway.Chunk );
                integration.IsIntegrated = true;

                OpenIntegration( gateway.Chunk, GridDirection.Down );
                OpenIntegration( gateway.Chunk, GridDirection.Up );
                OpenIntegration( gateway.Chunk, GridDirection.Left );
                OpenIntegration( gateway.Chunk, GridDirection.Right );
            }
        }

        private void OpenIntegration(int index, GridDirection direction)
        {
            var otherChunkIndex = GridUtility.GetNeighborIndex( index, direction, Pathfinder.NumberOfChunks );

            if ( !GridUtility.IsValid( otherChunkIndex ) )
                return;

            var thisIntegration = GetIntegration( index );
            var range = GridUtility.GetBorderRange( Pathfinder.ChunkSize, direction );
            var worldIndex = Pathfinder.CreateWorldPosition( index, 0 );
            var translation = new GridConverter( thisIntegration.Definition, Pathfinder.WorldSize, 0, worldIndex.WorldIndex );
            var indexes = new List<int>();

            for ( var x = range.XMin; x < range.XMax; x++ )
				for ( var y = range.YMin; y < range.YMax; y++ )
					indexes.Add(GridUtility.GetIndex(Pathfinder.ChunkSize, y, x));

            indexes.Sort( ( index1, index2 ) => thisIntegration.GetValue( index1 ).CompareTo( thisIntegration.GetValue( index2 ) ) );

            foreach ( var i in indexes )
            {
                var globalIndex = translation.Global(i);
                var thisCost = thisIntegration.GetValue(i);

                if ( thisCost == IntegrationService.Closed || thisCost == IntegrationService.UnIntegrated || thisCost < 0 )
                    continue;

                foreach ( var neighbor in GridUtility.GetNeighborsIndex( globalIndex, Pathfinder.WorldSize, true ) )
                {
                    var nw = Pathfinder.CreateWorldPosition(neighbor.Value);
                    var otherIntegration = GetIntegration(nw.ChunkIndex);

                    if ( otherIntegration.GetValue(nw.NodeIndex) != IntegrationService.UnIntegrated )
                        continue;

                    var otherCost = IntegrationService.H( thisCost, Pathfinder.GetCost( nw ) );

                    if (neighbor.Key == GridDirection.RightDown || neighbor.Key == GridDirection.UpRight
						|| neighbor.Key == GridDirection.LeftUp || neighbor.Key == GridDirection.DownLeft )
					{
						otherCost += 1;
					}

                    otherIntegration.SetValue( nw.NodeIndex, otherCost );
                    otherIntegration.Enqueue( nw.NodeIndex );
                }
            }
        }

        private void QueueFlow()
        {
            while ( _flowQueue.Count > 0 )
            {
                var chunkIndex = _flowQueue.Dequeue();
                if ( Flows.ContainsKey( chunkIndex ) ) continue;

                if ( _flowsToBeProcessed.Contains( chunkIndex ) )
                    continue;

                _flowsToBeProcessed.Add( chunkIndex );
                CalculateFlow( chunkIndex );
            }
        }

        public void CalculateFlow( int chunkIndex )
        {
			var flow = FlowService.Default.Flow(
				this,
				Pathfinder.WorldSize,
				Pathfinder.NumberOfChunks,
				Pathfinder.ChunkSize,
				chunkIndex
			);

			Flows.Add( chunkIndex, flow );

            _flowsToBeProcessed.Remove(chunkIndex);
        }

        public int GetIntegrationValue(GridWorldPosition position)
        {
            if ( !Integrations.ContainsKey( position.ChunkIndex ) ) return 0;
            return Integrations[position.ChunkIndex].GetValue( position.NodeIndex );
        }

        public int GetDirectionInt( GridWorldPosition position )
        {
            if ( !Flows.ContainsKey( position.ChunkIndex ) ) return -1;
            return Flows[position.ChunkIndex][position.NodeIndex];
        }

        public bool Ready( Vector3 position )
        {
            return Ready( Pathfinder.CreateWorldPosition( position ) );
        }

        public bool Ready( GridWorldPosition position )
        {
            if ( _flowsToBeProcessed.Contains( position.ChunkIndex ) )
                return false;

            if ( Flows.ContainsKey( position.ChunkIndex ) ) return true;

            SeekPath( Pathfinder.GetPosition( position ) );
            return false;
        }

        public Vector3 GetDirection( Vector3 position )
        {
            return GetDirection( Pathfinder.CreateWorldPosition( position ) );
        }

        public Vector3 GetDirection( GridWorldPosition position )
        {
            var gridDirection = GetGridDirection( position );

            if (gridDirection != GridDirection.Zero)
				return gridDirection.GetVector();

            foreach ( var gatewayLink in GatewayPath )
            {
                if (gatewayLink.Key.Chunk != position.ChunkIndex)
					continue;

                if ( gatewayLink.Key is Gateway connectionGateway )
                    return (connectionGateway.Portal.GetVector( Pathfinder ) - Pathfinder.GetPosition( position )).Normal;
            }

            return (DestinationPosition - Pathfinder.GetPosition( position )).Normal;
        }

        public GridDirection GetGridDirection( GridWorldPosition position )
        {
            if ( !Flows.ContainsKey(position.ChunkIndex) )
                return GridDirection.Zero;

            if ( Flows[position.ChunkIndex][position.NodeIndex] != -1 )
                return (GridDirection)Flows[position.ChunkIndex][position.NodeIndex];

            return GridDirection.Zero;
        }
    }
}
