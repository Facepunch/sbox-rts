using System;
using System.Collections;
using System.Collections.Generic;
using Gamelib.FlowFields.Grid;

namespace Gamelib.FlowFields.Connectors
{
    public sealed class Gateway : IEnumerable<int>, IComparable<Gateway>
	{
        public readonly Chunk Chunk;
        public readonly List<int> Nodes = new();
        public readonly Dictionary<Gateway, int> Connections = new();
        public readonly GridDirection Direction;
        public Portal Portal;
		public int CachedNodeCount;
		public int CachedMedian;
		public Dictionary<Portal, long> HeuristicLookup;

		public Gateway( Chunk chunk, GridDirection direction )
        {
            Chunk = chunk;
            Direction = direction;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Median()
        {
            return Nodes[CachedMedian];
        }

        public void AddNode( int node )
        {
            Nodes.Add( node );
			CachedNodeCount++;
			CachedMedian = (CachedNodeCount - CachedNodeCount % 2) / 2;
		}

        public override int GetHashCode()
        {
            return Chunk * 399 + Median();
        }

        public override string ToString()
        {
            return ((int)Chunk).ToString();
        }
        
        public override bool Equals( object obj )
        {
			if ( obj is not Gateway otherGateway ) return false;

			var median = Median();
            var otherMedian = otherGateway.Median();
            return median == otherMedian && Chunk == otherGateway.Chunk;
        }

		public int CompareTo( Gateway other )
		{
			return HeuristicLookup[Portal].CompareTo( other.HeuristicLookup[Portal] );
		}
	}
}
