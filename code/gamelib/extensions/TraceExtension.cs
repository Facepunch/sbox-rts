using Sandbox;

namespace Gamelib.Extensions
{
	public static class TraceExtension
	{
		public static Trace RayDirection( Vector3 from, Vector3 direction )
		{
			return Trace.Ray( from, from + direction.Normal * 100000f );
		}
	}
}
