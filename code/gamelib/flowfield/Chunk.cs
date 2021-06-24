using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Algorithms;
using Gamelib.FlowFields.Maths;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;

namespace Gamelib.FlowFields
{
    public class Chunk
    {
        public static byte Impassable = byte.MaxValue;

		private List<Gateway> _gateways = new();
		private List<Portal> _connectedPortals = new();
		private byte[] _costs;
        private bool[] _collisions;
		private GridDefinition _definition;
		private int _index;

        public bool IsDivided;
        public int Size => _definition.Size;
        public int Index => _index;

        public Chunk( int index, GridDefinition definition )
        {
            _definition = definition;
            _index = index;
            _costs = new byte[_definition.Size];
            _collisions = new bool[_definition.Size];
        }

        public static implicit operator int( Chunk chunk )
        {
            return chunk._index;
        }

        public bool IsImpassable( int index )
        {
            return GetCost(index) == Impassable || _collisions[index];
        }

        public bool HasCollision( int index )
        {
            return _collisions[index];
        }

        public void ClearCollisions()
        {
            _collisions = new bool[_definition.Size];
        }

        public void SetCollision( int index )
        {
            _costs[index] = Impassable;
            _collisions[index] = true;
        }

        public void RemoveCollision( int index )
        {
            _costs[index] = 0;
            _collisions[index] = false;
        }

        public int GetCost( int index )
        {
            return _collisions[index] ? Impassable : GetRawCost(index);
        }

        public int GetRawCost( int index )
        {
            return _costs[index];
        }

        public void SetCost( int index, byte cost )
        {
            _costs[index] = cost;
        }

        public void IncrementCost( int index )
        {
            _costs[index] = (byte)MathUtility.Clamp( _costs[index] + 10, byte.MinValue, byte.MaxValue );
        }

        public void DecrementCost( int index )
        {
            _costs[index] = (byte)MathUtility.Clamp( _costs[index] - 10, byte.MinValue, byte.MaxValue );
        }

        public void ClearGateways( GridDirection direction = GridDirection.Zero )
        {
			if ( _gateways == null )
				_gateways = new();

            if (direction == GridDirection.Zero)
                _gateways.Clear();
            else
                _gateways.RemoveAll( gateway => gateway.Direction == direction );
        }

        public void AddGateway( Gateway connectionGateway )
        {
            _gateways.Add( connectionGateway );
        }

        public bool IsInitialized()
        {
            return _gateways != null;
        }

        public bool HasGateway( int index )
        {
            return _gateways.Any( gateway => gateway.Contains(index) );
        }

        public void ConnectGateways()
        {
            IsDivided = false;

            foreach ( var gateway in _gateways )
                gateway.Connections.Clear();

            for ( var i = 0; i < _gateways.Count; i++ )
            for ( var j = i + 1; j < _gateways.Count; j++ )
            {
                var gateway1 = _gateways[i];
                var gateway2 = _gateways[j];

                var path = AStarGateway.Default.GetPath(
                    _definition,
                    _costs,
                    gateway1.Median(),
                    gateway2.Median()
                );

                if ( path == null )
                {
                    IsDivided = true;
                    continue;
                }

                var cost = AStarGateway.GetPathCost( path, _costs );

                if ( !gateway1.Connections.ContainsKey( gateway2 ) )
                    gateway1.Connections.Add( gateway2, cost );
                if ( !gateway2.Connections.ContainsKey( gateway1 ) )
                    gateway2.Connections.Add( gateway1, cost );
            }
        }

        public bool Connects( Gateway gateway, List<int> nodes )
        {
            return AStarGateway.Default.GetPath( _definition, _costs, gateway.Median(), nodes[0] ) != null;
        }

        public List<Gateway> GetGateways()
        {
            return _gateways.ToList();
        }

        public List<Gateway> GetGatewaysToChunk( int index )
        {
            return _gateways.Where( gateway => gateway.Portal.HasChunk(index) ).ToList();
        }

        public List<Portal> GetConnectedPortals( int index )
        {
            if (_connectedPortals == null)
                _connectedPortals = new();

            _connectedPortals.Clear();

            if ( IsDivided )
                _connectedPortals.AddRange( from gateway in _gateways
                    where AStarGateway.Default.GetPath(_definition, _costs, gateway.Median(), index) != null
                    select gateway.Portal );
            else
                _connectedPortals.AddRange( _gateways.Select(gateway => gateway.Portal) );

            return _connectedPortals;
        }
    }
}
