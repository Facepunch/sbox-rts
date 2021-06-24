using System.Collections.Generic;
using Gamelib.FlowFields.Grid;

namespace Gamelib.FlowFields.Algorithms
{
    public class FlowService
    {
        private static FlowService _default;
        public static FlowService Default => _default ?? (_default = new FlowService());

        private static int[] CreateFlowArray( GridDefinition definition )
        {
            var flow = new int[definition.Size];
            for (var i = 0; i < definition.Size; i++) flow[i] = 0;

            return flow;
        }

        private static bool IsIntegrationValid( IIntegration integration, int node )
        {
            return GridUtility.IsValid( node )
                   && integration.GetValue( node ) != IntegrationService.UnIntegrated
                   && integration.GetValue( node ) != IntegrationService.Closed;
        }

        public int[] Flow( IIntegrationContainer container, GridDefinition worldDefinition, GridDefinition definition, GridDefinition chunkDefinition, int chunkIndex )
        {
            var flowArray = CreateFlowArray( chunkDefinition );
            var pathfinder = container.GetPathfinder();

            if ( !container.HasIntegration( chunkIndex ) )
                return flowArray;

            var integration = container.GetIntegration( chunkIndex );
            var integrations = new Dictionary<int, Integration> {{chunkIndex, integration}};
            var neighbors = new List<GridNeighbor>( 8 );

            foreach ( var nIntegration in GridUtility.GetNeighborsIndex( chunkIndex, definition, true ) )
                integrations.Add(nIntegration.Value, container.GetIntegration(nIntegration.Value));

            for ( var i = 0; i < chunkDefinition.Size; i++ )
            {
                if ( !IsIntegrationValid( integration, i ) )
                {
                    flowArray[i] = -1;
                    continue;
                }

                var worldPosition = pathfinder.CreateWorldPosition( chunkIndex, i );
                var impassable = integrations[chunkIndex].GetValue( i ) < 0;

                neighbors.Clear();

                GridUtility.GetNeighborsIndexNonAlloc( worldPosition.WorldIndex, worldDefinition, neighbors, true );

                flowArray[i] = 0;
                var lowestValue = int.MaxValue;
            
                foreach ( var neighbor in neighbors )
                {
                    var neighborChunk = pathfinder.GetChunkIndex(neighbor.Index);
                    var neighborIndex = pathfinder.GetNodeIndex(neighbor.Index);

                    if ( !IsIntegrationValid( integrations[neighborChunk], neighborIndex ) )
                        continue;

                    var value = integrations[neighborChunk].GetValue( neighborIndex );

                    if ( impassable )
                    {
                        if ( lowestValue <= 0 || value < 0 || value >= lowestValue ) 
                            continue;
                        
                        flowArray[i] = (int)neighbor.Direction;
                        lowestValue = value;
                    }
                    else
                    {
                        if ( value < 0 )
                            continue;

                        if ( value < lowestValue )
                        {
                            flowArray[i] = (int)neighbor.Direction;
                            lowestValue = value;
                            continue;
                        }

                        if ( lowestValue <= value )
                            continue;

                        lowestValue = value;
                        flowArray[i] = (int)neighbor.Direction;
                    }
                }
            }

            return flowArray;
        }
    }
}
