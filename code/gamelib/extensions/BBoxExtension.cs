using System;
using Sandbox;

namespace Gamelib.Extensions
{
	public static class BBoxExtension
	{
		public static bool ContainsXY( this BBox a, BBox b )
		{
			return (
				b.Mins.x >= a.Mins.x && b.Maxs.x < a.Maxs.x &&
				b.Mins.y >= a.Mins.y && b.Maxs.y < a.Maxs.y
			); ;
		}

		public static bool Contains( this BBox a, BBox b )
		{
			return (
				b.Mins.x >= a.Mins.x && b.Maxs.x < a.Maxs.x &&
				b.Mins.y >= a.Mins.y && b.Maxs.y < a.Maxs.y &&
				b.Mins.z >= a.Mins.z && b.Maxs.z < a.Maxs.z
			); ;
		}

		public static bool Overlaps( this BBox a, BBox b )
		{
			return (
				a.Mins.x < b.Maxs.x && b.Mins.x < a.Maxs.x &&
				a.Mins.y < b.Maxs.y && b.Mins.y < a.Maxs.y &&
				a.Mins.z < b.Maxs.z && b.Mins.z < a.Maxs.z
			); ;
		}

		public static BBox ToWorldSpace( this BBox bbox, ModelEntity entity )
		{
			return new BBox
			{
				Mins = entity.Transform.PointToWorld( bbox.Mins ),
				Maxs = entity.Transform.PointToWorld( bbox.Maxs )
			};
		}
	}
}
