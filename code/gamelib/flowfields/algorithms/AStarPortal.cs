using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Gamelib.Data;
using Sandbox;

namespace Gamelib.FlowFields.Algorithms
{
    public sealed class AStarPortal
    {
        private static AStarPortal _default;

        private readonly Dictionary<Portal, long> _f = new();
        private readonly Dictionary<Portal, long> _g = new();

        private readonly HashSetList<Gateway> _openSet = new();
        private readonly HashSet<Gateway> _closedSet = new();
        
        private readonly Dictionary<Gateway, Gateway> _previous = new();
        public static AStarPortal Default => _default ?? (_default = new());

        private GatewaySubPath ReconstructPath( FlowField field, Gateway current )
        {
            var subPath = new GatewaySubPath {UntilConnectionGateway = current};

            while ( _previous.ContainsKey(current) )
            {
                if ( field.GatewayPath.ContainsKey( _previous[current]) )
                    break;

                field.GatewayPath.Add( _previous[current], current );

                if ( _previous[current].Equals( current ) )
                    break;

                current = _previous[current];
            }

            subPath.FromConnectionGateway = current;

            return subPath;
        }

        private int H( Pathfinder pathfinder, Gateway gateway, int worldIndex )
        {
            var from = pathfinder.CreateWorldPosition( gateway.Chunk, gateway.Median() );
            var to = worldIndex;
            return GridUtility.Distance( pathfinder.WorldGridSize, from.WorldIndex, to );
        }

        public GatewaySubPath CalculatePath( FlowField field, Vector3 startPosition )
        {
            SetupPath( field );
            return MergePathFrom( field, startPosition );
        }

        private void SetupPath( FlowField field )
        {
            _previous.Clear();
        }

        private GatewaySubPath MergePathFrom( FlowField field, Vector3 startPosition )
        {
            var worldPosition = field.Pathfinder.CreateWorldPosition( startPosition );

			_closedSet.Clear();
            _openSet.Clear();

            var chunk = field.Pathfinder.GetChunk( worldPosition.ChunkIndex );
            if ( chunk == null ) return null;

            var portals = chunk.GetConnectedPortals( worldPosition.NodeIndex );

			for ( int i = 0; i < portals.Count; i++ )
			{
				var portal = portals[i];
				var gateway = portal.GetGatewayInChunk( chunk );

				_openSet.Add( gateway );
			}

			_f.Clear();
            _g.Clear();

			for ( int i = 0; i < field.Pathfinder.Portals.Count; i++ )
            {
				var portal = field.Pathfinder.Portals[i];
				_f.Add( portal, long.MaxValue );
                _g.Add( portal, long.MaxValue );
            }

			for ( int i = 0; i < _openSet.Count; i++ )
            {
				var openGateway = _openSet[i];
				_g[openGateway.Portal] = 0;
                _f[openGateway.Portal] = H( field.Pathfinder, openGateway, field.DestinationIndex );
            }

            while ( _openSet.Count > 0 )
            {
                _openSet.Sort( (index1, index2) => _f[index1.Portal].CompareTo( _f[index2.Portal] ) );

				var current = _openSet[0];
                var oppositeGateway = current.Portal.OppositeGateway(current);

                if ( field.GatewayPath.ContainsKey( current ) )
					return ReconstructPath( field, current );

                if ( field.DestinationGateways.ContainsKey( oppositeGateway.Chunk ) &&
                    field.DestinationGateways[oppositeGateway.Chunk].Contains( oppositeGateway ))
                {
                    _previous[oppositeGateway] = current;
                    return ReconstructPath( field, oppositeGateway );
                }

                _openSet.Remove( current );
                _closedSet.Add( current );

                AddGatewayConnections( field, current );
            }

            return null;
        }

        private void AddGatewayConnections( FlowField field, Gateway connectionGateway )
        {
            var opposite = connectionGateway.Portal.OppositeGateway( connectionGateway );

            foreach ( var connection in opposite.Connections )
            {
                var connectedGateway = connection.Key;

                if ( _closedSet.Contains( connectedGateway ) )
                    continue;

                if ( _openSet.Contains( connectedGateway ) )
                    continue;

                var tentativeGScore = _g[connectionGateway.Portal] + connection.Value;
                if ( tentativeGScore >= _g[connectedGateway.Portal] ) continue;

                _previous[connectedGateway] = connectionGateway;
                _g[connectedGateway.Portal] = tentativeGScore;
                _f[connectedGateway.Portal] = _g[connectedGateway.Portal] + H( field.Pathfinder, connectedGateway, field.DestinationIndex );

                _openSet.Add( connectedGateway );
            }
        }
    }
}
