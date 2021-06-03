using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.Extensions
{
	public static class ListExtension
	{
		public static List<T> Shuffle<T>( this List<T> ts )
		{
			var count = ts.Count;
			var last = count - 1;

			for ( var i = 0; i < last; ++i )
			{
				var r = Rand.Int( i, last );
				var tmp = ts[i];
				ts[i] = ts[r];
				ts[r] = tmp;
			}

			return ts;
		}
	}
}
