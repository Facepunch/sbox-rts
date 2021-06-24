using Gamelib.FlowFields.Maths;
using System;

namespace Gamelib.FlowFields.Grid
{
    public class GridDefinition
    {
        public static GridDefinition Zero = new();

        public int Rows;
        public int Columns;

        public GridDefinition()
        {
        }

        public GridDefinition( Vector2i size )
        {
            Rows = size.y;
            Columns = size.x;
        }

        public GridDefinition( int rows, int columns )
        {
            Rows = rows;
            Columns = columns;
        }

        public int Size => Rows * Columns;

        public GridDefinition Scaled( long scale )
        {
            return new GridDefinition( MathUtility.CeilToInt( Rows / scale ), MathUtility.CeilToInt( Columns / scale ) );
        }

        public int GetIndex( int row, int column )
        {
            return row * Columns + column;
        }

        public Vector2i GetCoordinates( int index )
        {
            return index == 0 ? new Vector2i( 0, 0 ) : new Vector2i( index % Columns, index / Columns );
        }
    }
}
