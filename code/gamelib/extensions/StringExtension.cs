using Sandbox;
using Sandbox.Diagnostics;
using System;

namespace Gamelib.Extensions
{
	public static class StringExtension
	{
		public static Vector3 ToVector3( this string self )
		{
			var split = self.Split( ',' );
			Assert.True( split.Length == 3 );
			return new Vector3( split[0].ToFloat(), split[1].ToFloat(), split[2].ToFloat() );
		}
	}
}
