using Gamelib.FlowFields.Maths;
using System;
using System.Collections.Generic;

namespace Gamelib.FlowFields.Grid
{
    public enum GridDirection
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        RightDown = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        LeftUp = 7,
        Zero = 8
    }


    public static class GridDirectionExtensions
    {
        private static readonly Dictionary<GridDirection, Vector2i> Vector2ds = new()
        {
            {GridDirection.Up, new Vector2i( 0, 1 )},
            {GridDirection.UpRight, new Vector2i( 1, 1 )},
            {GridDirection.Right, new Vector2i( 1, 0 )},
            {GridDirection.RightDown, new Vector2i( 1, -1 )},
            {GridDirection.Down, new Vector2i( 0, -1 )},
            {GridDirection.DownLeft, new Vector2i( -1, -1 )},
            {GridDirection.Left, new Vector2i( -1, 0 )},
            {GridDirection.LeftUp, new Vector2i( -1, 1 )},
            {GridDirection.Zero, new Vector2i( 0, 0 )}
        };

        public static bool IsNeighbor( this GridDirection direction, GridDirection compare )
        {
            if ( (int) direction == (int) compare )
                return true;

            var min = (int)direction - 1;
            if ( min < 0 ) min = 7;

            if ( min == (int)compare ) return true;

            var max = (int)direction + 1;
            if ( max > 7 ) max = 0;

            return max == (int)compare;
        }

        public static GridDirection Opposite( this GridDirection direction )
        {
            switch ( direction )
            {
                case GridDirection.Up:
                    return GridDirection.Down;
                case GridDirection.Right:
                    return GridDirection.Left;
                case GridDirection.Down:
                    return GridDirection.Up;
                case GridDirection.Left:
                    return GridDirection.Right;
                case GridDirection.UpRight:
                    break;
                case GridDirection.RightDown:
                    break;
                case GridDirection.DownLeft:
                    break;
                case GridDirection.LeftUp:
                    break;
                case GridDirection.Zero:
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof(direction), direction, null );
            }

            return GridDirection.Down;
        }

        public static Vector3 GetVector( this GridDirection direction )
        {
            if ( !Vector2ds.ContainsKey(direction) )
                return Vector3.Zero;

            return new Vector3( Vector2ds[direction].x, Vector2ds[direction].y, 0 );
        }
    }
}
