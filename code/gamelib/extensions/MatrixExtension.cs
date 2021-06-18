using Sandbox.UI;
using Sandbox;
using System.Numerics;

namespace Gamelib.Extensions
{
	public static class MatrixExtension
	{
		public static Matrix Compose( Vector3 position, Rotation rotation, Vector3 scale )
		{
			return Matrix.CreateTranslation( position ) * Matrix.CreateRotation( rotation ) * Matrix.CreateScale( scale );
		}

		public static Matrix View( Vector3 position, Rotation rotation )
		{
			var matrix = Matrix.CreateRotation( Rotation.FromAxis( new Vector3( 1f, 0f, 0f ), -90f ) );

			var rotationMatrix = Matrix.CreateRotation( Rotation.FromAxis( new Vector3( 0f, 0f, 1f ), 90f ) );
			matrix *= rotationMatrix;

			var rotationMatrix1 = Matrix.CreateRotation( Rotation.FromAxis( new Vector3( 1f, 0f, 0f ), -rotation.z ) );
			matrix *= rotationMatrix1;

			var rotationMatrix2 = Matrix.CreateRotation( Rotation.FromAxis( new Vector3( 0f, 1f, 0f ), -rotation.x ) );
			matrix *= rotationMatrix2;

			var rotationMatrix3 = Matrix.CreateRotation( Rotation.FromAxis( new Vector3( 0f, 0f, 1f ), -rotation.y ) );
			matrix *= rotationMatrix3;

			var translation = Matrix.CreateTranslation( -position );
			matrix *= translation;

			return matrix;
		}

		public static Matrix Transpose( this Matrix self )
		{
			return Matrix4x4.Transpose( self.Numerics );
		}

		public static Matrix Ortho( float left, float top, float right, float bottom, float near, float far )
		{
			var matrix = new Matrix();

			matrix.Numerics.M11 = 2.0f / (right - left);
			matrix.Numerics.M12 = 0f;
			matrix.Numerics.M13 = 0f;
			matrix.Numerics.M14 = (left + right) / (left - right);

			matrix.Numerics.M21 = 0f;
			matrix.Numerics.M22 = 2.0f / (bottom - top);
			matrix.Numerics.M23 = 0f;
			matrix.Numerics.M24 = (bottom + top) / (top - bottom);

			matrix.Numerics.M31 = 0f;
			matrix.Numerics.M32 = 0f;
			matrix.Numerics.M33 = 1.0f / (near - far);
			matrix.Numerics.M34 = near / (near - far);

			matrix.Numerics.M41 = 0f;
			matrix.Numerics.M42 = 0f;
			matrix.Numerics.M43 = 0f;
			matrix.Numerics.M44 = 1f;

			return matrix;
		}
	}
}
