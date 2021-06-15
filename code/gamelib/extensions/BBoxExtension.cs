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

		public static bool Overlaps( this BBox bbox, Vector3 position, float radius )
		{
			var dmin = 0f;
			var bmin = bbox.Mins;
			var bmax = bbox.Maxs;

			if ( position.x < bmin.x )
			{
				dmin += MathF.Pow( position.x - bmin.x, 2 );
			}
			else if ( position.x > bmax.x )
			{
				dmin += MathF.Pow( position.x - bmax.x, 2 );
			}

			if ( position.y < bmin.y )
			{
				dmin += MathF.Pow( position.y - bmin.y, 2 );
			}
			else if ( position.y > bmax.y )
			{
				dmin += MathF.Pow( position.y - bmax.y, 2 );
			}

			if ( position.z < bmin.z )
			{
				dmin += MathF.Pow( position.z - bmin.z, 2 );
			}
			else if ( position.z > bmax.z )
			{
				dmin += MathF.Pow( position.z - bmax.z, 2 );
			}

			return dmin <= MathF.Pow( radius, 2 );
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
