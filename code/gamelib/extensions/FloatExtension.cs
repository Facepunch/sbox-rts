using System;

namespace Gamelib.Extensions
{
	public static class FloatExtension
	{
		public static float Normalize( this float value, float min, float max )
		{
			var width = max - min;
			var offset = value - min;
			return (offset - (MathF.Floor( offset / width ) * width)) + min;
		}

		public static float AsPercentMinMax( this float value, float min, float max )
		{
			return ((value - min) * 100f) / (max - min);
		}
	}
}
