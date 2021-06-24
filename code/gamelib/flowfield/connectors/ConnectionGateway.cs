using System.Collections;
using System.Collections.Generic;
using Gamelib.FlowFields.Grid;

namespace Gamelib.FlowFields.Connectors
{
    public sealed class Gateway : IEnumerable<int>
    {
        public readonly Chunk Chunk;
        public readonly List<int> Nodes = new();
        public readonly Dictionary<Gateway, int> Connections = new();
        public readonly GridDirection Direction;
        public Portal Portal;
        
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
            return Nodes[(Nodes.Count - Nodes.Count % 2) / 2];
        }

        public void AddNode( int node )
        {
            Nodes.Add( node );
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
    }
}
