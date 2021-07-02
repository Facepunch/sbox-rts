using System;
using Sandbox;

namespace Gamelib.FlowFields.Maths
{
    public static class MathUtility
	{
		public static int CeilToInt( float value )
		{
			return value.CeilToInt();
		}

		public static int RoundToInt( float value )
		{
			return (int)MathF.Round( value );
		}

		public static int FloorToInt( float value )
		{
			return value.FloorToInt();
		}

		public static float Clamp( float value, float min, float max )
		{
			return value.Clamp( min, max );
		}
	}
}
