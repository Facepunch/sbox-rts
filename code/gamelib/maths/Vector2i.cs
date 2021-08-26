using Sandbox;
using System;

namespace Gamelib.Maths
{
    public readonly struct Vector2i : IEquatable<Vector2i>
    {
		public static Vector2i Zero = new( 0, 0 );
        public readonly int x;
        public readonly int y;

        public Vector2i( int x, int y )
        {
            this.x = x;
            this.y = y;
        }

		public Vector2i( Vector3 position )
		{
			x = position.x.CeilToInt();
			y = position.y.CeilToInt();
		}

        public static Vector2i operator *( Vector2i a, Vector2i b )
        {
            return new Vector2i( a.x * b.x, a.y * b.y );
        }

        public static Vector2i operator +( Vector2i a, Vector2i b )
        {
            return new Vector2i( a.x + b.x, a.y + b.y );
        }

        public static Vector2i operator -( Vector2i a, Vector2i b )
        {
            return new Vector2i( a.x - b.x, a.y - b.y );
        }

        public static Vector2i operator /( Vector2i a, int b )
        {
            return new Vector2i( a.x / b, a.y / b );
        }

        public static Vector2i operator *( Vector2i a, int b )
        {
            return new Vector2i( a.x * b, a.y * b );
        }

        public static Vector2i operator -( Vector2i a, int b )
        {
            return new Vector2i( a.x - b, a.y - b );
        }

        public static Vector2i operator +( Vector2i a, int b )
        {
            return new Vector2i( a.x + b, a.y + b );
        }

        public static bool operator ==( Vector2i a, Vector2i b )
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=( Vector2i a, Vector2i b )
        {
            return a.x != b.x || a.y != b.y;
        }

        public override bool Equals( object o )
        {
            if ( o == null ) return false;
            var rhs = (Vector2i)o;
            return x == rhs.x && y == rhs.y;
        }

        public bool Equals( Vector2i other )
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x * 49157 + y * 98317;
        }

        public static Vector2i Min( Vector2i a, Vector2i b )
        {
            return new Vector2i( Math.Min( a.x, b.x ), Math.Min( a.y, b.y ) );
        }

        public static Vector2i Max( Vector2i a, Vector2i b )
        {
            return new Vector2i( Math.Max( a.x, b.x ), Math.Max( a.y, b.y ) );
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
