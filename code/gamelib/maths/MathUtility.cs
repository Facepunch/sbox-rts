using System;
using Sandbox;

namespace Gamelib.Maths
{
    public static class MathUtility
	{
		public static int CeilToInt( float value )
		{
			return value.CeilToInt();
		}

		public static int HashNumbers( short a, short b )
		{
			var ha = (uint)(a >= 0 ? 2 * a : -2 * a - 1);
			var hb = (uint)(b >= 0 ? 2 * b : -2 * b - 1);
			var hc = (int)((ha >= hb ? ha * ha + ha + hb : ha + hb * hb) / 2);
			return a < 0 && b < 0 || a >= 0 && b >= 0 ? hc : -hc - 1;
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
