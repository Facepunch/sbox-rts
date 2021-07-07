using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.FlowFields.Algorithms;
using Gamelib.FlowFields.Maths;
using Gamelib.FlowFields.Grid;
using Gamelib.FlowFields.Connectors;
using Sandbox;

namespace Gamelib.FlowFields
{
    public class Chunk
    {
        public const byte Impassable = byte.MaxValue;

		private List<Portal> _connectedPortals = new();
		private List<Gateway> _gateways = new();
        private readonly NodeCollision[] _collisions;
		private readonly byte[] _costs;
		private readonly GridDefinition _definition;
		private readonly int _index;
		private bool _isDivided;

		public NodeCollision[] Collisions => _collisions;
        public GridDefinition Definition => _definition;
        public int Size => _definition.Size;
        public int Index => _index;

        public Chunk( int index, GridDefinition definition )
        {
            _definition = definition;
            _index = index;
            _costs = new byte[_definition.Size];
            _collisions = new NodeCollision[_definition.Size];
        }

        public static implicit operator int( Chunk chunk )
        {
            return chunk._index;
        }

        public bool IsImpassable( int index )
        {
            return GetCost( index ) == Impassable || (_collisions[index] != NodeCollision.None);
        }

        public bool HasCollision( int index )
        {
            return (_collisions[index] != NodeCollision.None);
        }

		public NodeCollision GetCollision( int index )
		{
			return _collisions[index];
		}

        public void ClearCollisions()
        {
			Array.Clear( _collisions, 0, _collisions.Length );
        }

        public void SetCollision( int index, NodeCollision type = NodeCollision.Dynamic )
        {
            _collisions[index] = type;
            _costs[index] = Impassable;
        }

        public void RemoveCollision( int index )
        {
            _collisions[index] = NodeCollision.None;
            _costs[index] = 0;
        }

        public int GetCost( int index )
        {
            return (_collisions[index] != NodeCollision.None) ? Impassable : GetRawCost(index);
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
            _isDivided = false;

            foreach ( var gateway in _gateways )
			{
                gateway.Connections.Clear();
			}

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
                    _isDivided = true;
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
			_connectedPortals ??= new();
			_connectedPortals.Clear();

            if ( _isDivided )
                _connectedPortals.AddRange( from gateway in _gateways
                    where AStarGateway.Default.GetPath( _definition, _costs, gateway.Median(), index ) != null
                    select gateway.Portal );
            else
                _connectedPortals.AddRange( _gateways.Select( gateway => gateway.Portal ) );

            return _connectedPortals;
        }
    }
}
