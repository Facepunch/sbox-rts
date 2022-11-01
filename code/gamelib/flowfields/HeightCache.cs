using Gamelib.Maths;
using Sandbox;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public static class HeightCache
	{
		private static Dictionary<Vector2i, float> Cache = new();

		public static float GetHeight( Vector3 position )
		{
			var ceiled = new Vector2i( position );

			if ( Cache.TryGetValue( ceiled, out float height) )
			{
				return height;
			}

			var trace = Trace.Ray( position.WithZ( 1000f ), position.WithZ( -1000f ) )
				.WorldOnly()
				.Run();

			height = trace.EndPosition.z;
			Cache[ceiled] = height;
			return height;
		}
	}
}
