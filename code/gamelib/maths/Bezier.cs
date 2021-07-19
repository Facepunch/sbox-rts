using System;
using Sandbox;

namespace Gamelib.FlowFields.Maths
{
    public static class Bezier
	{
		public static Vector3 Calculate( Vector3 start, Vector3 middle, Vector3 end, float time )
		{
			var startToMiddle = (1f - time) * start + time * middle;
			var middleToEnd = (1f - time) * middle + time * end;
			var result = (1f - time) * startToMiddle + time * middleToEnd;
			return result;
		}
	}
}
