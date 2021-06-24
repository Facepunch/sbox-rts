using System;

namespace Gamelib.FlowFields.Grid
{
    public readonly struct GridWorldPosition : IEquatable<GridWorldPosition>
    {
        public readonly int WorldIndex;
        public readonly int ChunkIndex;
        public readonly int NodeIndex;

        public GridWorldPosition( int worldIndex, int chunkIndex, int nodeIndex )
        {
            WorldIndex = worldIndex;
            ChunkIndex = chunkIndex;
            NodeIndex = nodeIndex;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WorldIndex;
                hashCode = (hashCode * 397) ^ ChunkIndex;
                hashCode = (hashCode * 397) ^ NodeIndex;
                return hashCode;
            }
        }

        public bool Equals( GridWorldPosition other )
        {
            return WorldIndex == other.WorldIndex && ChunkIndex == other.ChunkIndex && NodeIndex == other.NodeIndex;
        }

        public static implicit operator string( GridWorldPosition obj )
        {
            return obj.ToString();
        }

        public override string ToString()
        {
            return "( " + WorldIndex + "," + ChunkIndex + "," + NodeIndex + ")";
        }
    }
}
