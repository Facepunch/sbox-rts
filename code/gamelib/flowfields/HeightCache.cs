using Gamelib.Maths;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamelib.FlowFields
{
    public static class HeightCache
	{
		private static Dictionary<Vector2i, float> _heightCache = new();

		public static float GetHeight( Vector3 position )
		{
			var ceiled = new Vector2i( position );

			if ( _heightCache.TryGetValue( ceiled, out float height) )
			{
				return height;
			}

			var trace = Trace.Ray( position.WithZ( 1000f ), position.WithZ( -1000f ) )
				.WorldOnly()
				.Run();

			height = trace.EndPosition.z;
			_heightCache[ceiled] = height;
			return height;
		}
	}
}
