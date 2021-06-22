using System;

namespace Gamelib.Math
{
	public struct Vector2i
	{
		public int x;
		public int y;

		public Vector2i( int x, int y )
		{
			this.x = x;
			this.y = y;
		}

		public float LengthSquared
		{
			get { return (float)x * x + (float)y * y; }
		}

		public float Length
		{
			get { return MathF.Sqrt( LengthSquared ); }
		}

		public override string ToString()
		{
			return string.Format( "({0},{1})", x, y );
		}

		public static Vector2i operator +( Vector2i a, Vector2i b )
		{
			return new Vector2i(
				a.x + b.x,
				a.y + b.y
			);
		}

		public static Vector2i operator -( Vector2i a )
		{
			return new Vector2i(
				-a.x,
				-a.y
			);
		}

		public static Vector2i operator -( Vector2i a, Vector2i b )
		{
			return a + (-b);
		}

		public static Vector2i operator *( int d, Vector2i a )
		{
			return new Vector2i(
				d * a.x,
				d * a.y
			);
		}

		public static Vector2i operator *( Vector2i a, int d )
		{
			return d * a;
		}

		public static Vector2i operator /( Vector2i a, int d )
		{
			return new Vector2i(
				a.x / d,
				a.y / d
			);
		}

		public static bool operator ==( Vector2i lhs, Vector2i rhs )
		{
			return lhs.x == rhs.x && lhs.y == rhs.y;
		}

		public static bool operator !=( Vector2i lhs, Vector2i rhs )
		{
			return !(lhs == rhs);
		}

		public override bool Equals( object other )
		{
			if ( !(other is Vector2i) )
			{
				return false;
			}
			return this == (Vector2i)other;
		}

		public bool Equals( Vector2i other )
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return (x.GetHashCode() << 6) ^ y.GetHashCode();
		}

		public static float Distance( Vector2i a, Vector2i b )
		{
			return (a - b).Length;
		}

		public static Vector2i Min( Vector2i lhs, Vector2i rhs )
		{
			return new Vector2i(
				System.Math.Min( lhs.x, rhs.x ),
				System.Math.Min( lhs.y, rhs.y )
			);
		}

		public static Vector2i Max( Vector2i a, Vector2i b )
		{
			return new Vector2i(
				System.Math.Max( a.x, b.x ),
				System.Math.Max( a.y, b.y )
			);
		}

		public static int Dot( Vector2i lhs, Vector2i rhs )
		{
			return lhs.x * rhs.x +
				   lhs.y * rhs.y;
		}

		public static float Magnitude( Vector2i a )
		{
			return a.Length;
		}

		public static float SqrMagnitude( Vector2i a )
		{
			return a.LengthSquared;
		}

		public static Vector2i Down
		{
			get { return new Vector2i( 0, -1 ); }
		}

		public static Vector2i Up
		{
			get { return new Vector2i( 0, 1 ); }
		}

		public static Vector2i Left
		{
			get { return new Vector2i( -1, 0 ); }
		}

		public static Vector2i Right
		{
			get { return new Vector2i( 1, 0 ); }
		}

		public static Vector2i One
		{
			get { return new Vector2i( 1, 1 ); }
		}

		public static Vector2i Zero
		{
			get { return new Vector2i( 0, 0 ); }
		}

		public static explicit operator Vector2i( Vector2 source )
		{
			return new Vector2i( (int)source.x, (int)source.y );
		}

		public static implicit operator Vector2( Vector2i source )
		{
			return new Vector2( source.x, source.y );
		}

		public static explicit operator Vector2i( Vector3 source )
		{
			return new Vector2i( (int)source.x, (int)source.y );
		}

		public static implicit operator Vector3( Vector2i source )
		{
			return new Vector3( source.x, source.y, 0 );
		}
	}
}
