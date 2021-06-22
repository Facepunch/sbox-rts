using System.Collections.Generic;
using System.Linq;
using Gamelib.Math;

namespace Gamelib.FlowField
{
	public class GridDirection
	{
		public readonly Vector2i Direction;

		private GridDirection( int x, int y )
		{
			Direction = new Vector2i( x, y );
		}

		public static implicit operator Vector2i( GridDirection direction )
		{
			return direction.Direction;
		}

		public static GridDirection GetDirectionFromVector( Vector2i vector )
		{
			return CardinalAndIntercardinalDirections.DefaultIfEmpty( None ).FirstOrDefault( direction => direction == vector );
		}

		public static readonly GridDirection None = new ( 0, 0 );
		public static readonly GridDirection North = new( 0, 1 );
		public static readonly GridDirection South = new( 0, -1 );
		public static readonly GridDirection East = new( 1, 0 );
		public static readonly GridDirection West = new( -1, 0 );
		public static readonly GridDirection NorthEast = new( 1, 1 );
		public static readonly GridDirection NorthWest = new( -1, 1 );
		public static readonly GridDirection SouthEast = new( 1, -1 );
		public static readonly GridDirection SouthWest = new( -1, -1 );

		public static readonly List<GridDirection> CardinalDirections = new()
		{
			North,
			East,
			South,
			West
		};
		
		public static readonly List<GridDirection> CardinalAndIntercardinalDirections = new()
		{
			North,
			NorthEast,
			East,
			SouthEast,
			South,
			SouthWest,
			West,
			NorthWest
		};

		public static readonly List<GridDirection> AllDirections = new()
		{
			None,
			North,
			NorthEast,
			East,
			SouthEast,
			South,
			SouthWest,
			West,
			NorthWest
		};
	}
}
