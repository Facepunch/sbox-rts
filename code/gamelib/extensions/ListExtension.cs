using Sandbox;
using System;
using System.Collections.Generic;

namespace Gamelib.Extensions
{
	public static class ListExtension
	{
		public static List<T> Shuffle<T>( this List<T> self )
		{
			var count = self.Count;
			var last = count - 1;

			for ( var i = 0; i < last; ++i )
			{
				var r = Game.Random.Int( i, last );
				var tmp = self[i];
				self[i] = self[r];
				self[r] = tmp;
			}

			return self;
		}

		private readonly static int HashCodeSeed = 17;
		private readonly static int HashCodeMultiplier = 23;

		public static int GetItemsHashCode<T>( this List<T> self )
		{
			var hashCode = HashCodeSeed;

			for ( int index = 0; index < self.Count; index++ )
			{
				if ( self[index] != null )
					hashCode = hashCode * HashCodeMultiplier + self[index].GetHashCode();
			}

			return hashCode;
		}
	}
}
