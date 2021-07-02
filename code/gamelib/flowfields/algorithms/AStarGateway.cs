using System.Collections.Generic;
using System.Linq;
using Gamelib.Data;
using Gamelib.FlowFields.Grid;
using Sandbox;

namespace Gamelib.FlowFields.Algorithms
{
    public sealed class AStarGateway
    {
        private static AStarGateway _default;
        private readonly HashSet<int> _closedSet = new();
        private readonly HashSetList<int> _openSet = new();
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
                path.Add(current);

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

            _closedSet.Clear();
            _previous.Clear();
            _openSet.Clear();

			if ( end.Contains( start ) )
			{
				return new List<int> { start };
			}

            _openSet.Add(start);

            while ( _openSet.Count > 0 )
            {
                _openSet.Sort( CompareValues );

				var current = _openSet[0];

                if ( end.Contains( current ) )
				{
					return ReconstructPath( current );
				}

                _openSet.Remove( current );
                _closedSet.Add( current );

                foreach ( var neighborItem in GridUtility.GetNeighborsIndex( current, definition, true ) )
                {
                    var neighbor = neighborItem.Value;

                    if ( _closedSet.Contains( neighbor ) )
                        continue;

                    if ( _openSet.Contains( neighbor ) )
                        continue;

                    if ( !GridUtility.IsValid( neighbor ) || costs[neighbor] == Chunk.Impassable )
                    {
                        _closedSet.Add( neighbor );
                        continue;
                    }

                    var tentativeGScore = _g[current] + costs[neighbor] + D();
                    if (tentativeGScore >= _g[neighbor]) continue;

                    _previous[neighbor] = current;
                    _g[neighbor] = tentativeGScore;
                    _f[neighbor] = _g[neighbor] + H( neighbor );
                    _openSet.Add( neighbor );
                }
            }

			return null;
        }

		private int CompareValues( int a, int b )
		{
			return _f[a].CompareTo( _f[b] );
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
