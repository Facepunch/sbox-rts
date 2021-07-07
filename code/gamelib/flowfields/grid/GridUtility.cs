using Gamelib.FlowFields.Maths;
using System;
using System.Collections.Generic;
using static System.Int32;

namespace Gamelib.FlowFields.Grid
{
    public struct GridRange
    {
        public int MinY;
        public int MaxY;
        public int MinX;
        public int MaxX;
    }

    public readonly struct GridNeighbor
    {
        public readonly int Index;
        public readonly GridDirection Direction;

        public GridNeighbor( int index, GridDirection direction )
        {
            Index = index;
            Direction = direction;
        }
    }

    public static class GridUtility
    {
        private static readonly List<GridDirection> AllNeighbors = new();
        private static readonly List<GridDirection> DirectNeighbors = new();
        private static readonly Dictionary<GridDirection, int> Results = new();

        static GridUtility()
        {
            AllNeighbors.Add( GridDirection.Up );
            AllNeighbors.Add( GridDirection.Right );
            AllNeighbors.Add( GridDirection.Down );
            AllNeighbors.Add( GridDirection.Left );

            AllNeighbors.Add( GridDirection.UpRight );
            AllNeighbors.Add( GridDirection.RightDown );
            AllNeighbors.Add( GridDirection.DownLeft );
            AllNeighbors.Add( GridDirection.LeftUp );

            DirectNeighbors.Add( GridDirection.Up );
            DirectNeighbors.Add( GridDirection.Right );
            DirectNeighbors.Add( GridDirection.Down );
            DirectNeighbors.Add( GridDirection.Left );
        }

        public static int Distance( GridDefinition definition, int i, int j )
        {
            return Distance( GetCoordinates( definition, i ), GetCoordinates( definition, j ) );
        }

        private static int Distance( Vector2i i, Vector2i j )
        {
            var temp1 = i.x - j.x;
            temp1 *= temp1;
            var temp2 = i.y - j.y;
            temp2 *= temp2;
            return temp1 + temp2;
        }

        private static bool ValidateBounds( int index, GridDefinition definition )
        {
            return index >= 0 && index < definition.Size;
        }

        private static bool ValidateRow( int index, int offset, GridDefinition definition )
        {
            return index % definition.Columns + offset < definition.Columns && index % definition.Columns + offset >= 0;
        }

        public static bool IsValid( int index )
        {
            return MinValue != index;
        }

        public static void GetNeighborsIndexNonAlloc( int index, GridDefinition definition, List<GridNeighbor> results, bool diagonal = false )
        {
            results.Clear();

			var list = GetGridDirections( diagonal );

			for ( int i = 0; i < list.Count; i++ )
            {
				var direction = list[i];
				var neighborIndex = GetNeighborIndex( index, direction, definition );

                if ( !IsValid( neighborIndex ) )
                    continue;

                results.Add( new GridNeighbor( neighborIndex, direction ) );
            }
        }

        public static Dictionary<GridDirection, int> GetNeighborsIndex( int index, GridDefinition definition, bool diagonal = false )
        {
            Results.Clear();

            foreach ( var direction in GetGridDirections( diagonal ) )
            {
                var neighborIndex = GetNeighborIndex( index, direction, definition );

                if ( !IsValid( neighborIndex ) )
                    continue;

                Results.Add( direction, neighborIndex );
            }

            return Results;
        }

        private static List<GridDirection> GetGridDirections( bool diagonal )
        {
            return diagonal ? AllNeighbors : DirectNeighbors;
        }

        public static int GetNeighborIndex( int index, GridDirection direction, GridDefinition definition )
        {
            var neighborIndex = MinValue;

            switch (direction)
            {
                case GridDirection.Up:
                    neighborIndex = index + definition.Columns;
                    break;

                case GridDirection.UpRight:
                    if ( !ValidateRow( index, 1, definition ) )
                        break;

                    neighborIndex = index + definition.Columns + 1;
                    break;

                case GridDirection.Right:
                    if ( !ValidateRow( index, 1, definition ) )
                        break;

                    neighborIndex = index + 1;
                    break;

                case GridDirection.RightDown:
                    if ( !ValidateRow( index, 1, definition ) )
                        break;

                    neighborIndex = index - definition.Columns + 1;
                    break;

                case GridDirection.Down:
                    neighborIndex = index - definition.Columns;
                    break;

                case GridDirection.DownLeft:
                    if ( !ValidateRow( index, -1, definition ) )
                        break;

                    neighborIndex = index - definition.Columns - 1;
                    break;
                case GridDirection.Left:
                    if ( !ValidateRow( index, -1, definition ) )
                        break;

                    neighborIndex = index - 1;
                    break;

                case GridDirection.LeftUp:
                    if ( !ValidateRow( index, -1, definition ) )
                        break;

                    neighborIndex = index + definition.Columns - 1;
                    break;
            }

            return !ValidateBounds( neighborIndex, definition ) ? MinValue : neighborIndex;
        }

        public static int GetMirrorIndex( GridDefinition definition, int index, GridDirection direction )
        {
            switch (direction)
            {
                case GridDirection.Right:
                    return Math.Abs( index - (definition.Columns - 1) );
                case GridDirection.Up:
                    return Math.Abs( index - (definition.Columns - 1) * definition.Rows );
                case GridDirection.Left:
                    return Math.Abs( index + (definition.Columns - 1) );
                case GridDirection.Down:
                    return Math.Abs( index + (definition.Columns - 1) * definition.Rows );
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
                    return MinValue;
            }

            return MinValue;
        }

        public static int GetIndex( GridDefinition definition, int row, int column )
        {
            if ( row < 0 || row >= definition.Rows || column < 0 || column >= definition.Columns )
                return MinValue;

            return row * definition.Columns + column;
        }

        public static Vector2i GetCoordinates( GridDefinition definition, int index )
        {
            return index == 0 ? new Vector2i( 0, 0 ) : new Vector2i( index % definition.Columns, index / definition.Columns );
        }

        public static GridRange GetBorderRange( GridDefinition definition, GridDirection direction )
        {
            var range = new GridRange();

            switch (direction)
            {
                case GridDirection.Up:
                    range.MaxY = definition.Rows;
                    range.MinY = range.MaxY - 1;

                    range.MinX = 0;
                    range.MaxX = definition.Columns;
                    break;
                case GridDirection.Right:
                    range.MaxY = definition.Rows;
                    range.MinY = 0;

                    range.MaxX = definition.Columns;
                    range.MinX = range.MaxX - 1;
                    break;
                case GridDirection.Down:
                    range.MinY = 0;
                    range.MaxY = range.MinY + 1;

                    range.MinX = 0;
                    range.MaxX = definition.Columns;
                    break;
                case GridDirection.Left:
                    range.MaxY = definition.Rows;
                    range.MinY = 0;

                    range.MinX = 0;
                    range.MaxX = range.MinX + 1;
                    break;
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

            return range;
        }
    }
}
