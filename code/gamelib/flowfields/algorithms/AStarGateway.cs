using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.Data;
using Gamelib.FlowFields.Grid;
using Sandbox;

namespace Gamelib.FlowFields.Algorithms
{
	public struct GatewayNode : IComparable<GatewayNode>, IEquatable<GatewayNode>
	{
		public int[] heuristics;
		public int index;

		public int CompareTo( GatewayNode other )
		{
			var a = heuristics[index];
			var b = heuristics[other.index];
			if ( a < b ) return -1;
			if ( a > b ) return 1;
			return 0;
		}

		public bool Equals( GatewayNode other )
		{
			return other.index == index;
		}
	}

    public sealed class AStarGateway
    {
        private static AStarGateway _default;
        private readonly HashSet<int> _closedSet = new();
        private readonly HashSet<int> _processed = new();
        private readonly PriorityQueue<GatewayNode> _openSet = new();
        private readonly Dictionary<int, int> _previous = new();
        private GridDefinition _definition;

        private List<int> _end;

        private int[] _f;
        private int[] _g;
        public static AStarGateway Default => _default ?? (_default = new());

        private List<int> ReconstructPath( int current )
        {
            var path = new List<int>();

            while  ( _previous.ContainsKey( current ) )
            {
                path.Add( current );

                if ( _previous[current] == current )
					break;

                current = _previous[current];
            }

            return path;
        }

        public static int GetPathCost( IEnumerable<int> nodes, byte[] costs )
        {
            return nodes.Aggregate( 0, (current, node) => current + costs[node] );
        }

        public List<int> GetPath( GridDefinition definition, byte[] costs, int start, int end )
        {
            var ends = new List<int> { end };
            return GetPath( definition, costs, start, ends );
        }

		public List<int> GetPath( GridDefinition definition, byte[] costs, int start, List<int> end )
        {
            _end = end;
            _definition = definition;

            _f = new int[definition.Size];
            _g = new int[definition.Size];

            for ( var i = 0; i < _g.Length; i++ )
            {
                _g[i] = int.MaxValue;
                _f[i] = int.MaxValue;
            }

            _g[start] = 0;
            _f[start] = H( start );

			_processed.Clear();
			_closedSet.Clear();
            _previous.Clear();
            _openSet.Clear();

			if ( end.Contains( start ) )
			{
				return new List<int> { start };
			}

			_processed.Add( start );

			_openSet.Enqueue( new GatewayNode
			{
				index = start,
				heuristics = _f
			} );

            while ( _openSet.Data.Count > 0 )
            {
				var current = _openSet.Dequeue();

                if ( end.Contains( current.index ) )
				{
					return ReconstructPath( current.index );
				}

				_processed.Remove( current.index );
				_closedSet.Add( current.index );

                foreach ( var neighborItem in GridUtility.GetNeighborsIndex( current.index, definition, true ) )
                {
					var neighbor = new GatewayNode
					{
						index = neighborItem.Value,
						heuristics = _f
					};

					if ( _closedSet.Contains( neighbor.index ) )
                        continue;

                    if ( _processed.Contains( neighbor.index ) )
                        continue;

                    if ( !GridUtility.IsValid( neighbor.index ) || costs[neighbor.index] == Chunk.Impassable )
                    {
                        _closedSet.Add( neighbor.index );
                        continue;
                    }

                    var tentativeGScore = _g[current.index] + costs[neighbor.index] + D();
                    if (tentativeGScore >= _g[neighbor.index] ) continue;

                    _previous[neighbor.index] = current.index;
                    _g[neighbor.index] = tentativeGScore;
                    _f[neighbor.index] = _g[neighbor.index] + H( neighbor.index );

					_processed.Add( neighbor.index );
					_openSet.Enqueue( neighbor );
                }
            }

			return null;
        }

        private int H( int i )
        {
            return GridUtility.Distance( _definition, i, _end[0] );
        }

        private static int D()
        {
            return 1;
        }
    }
}
