using System;
using System.Collections.Generic;
using Sandbox;

namespace Gamelib.Nav
{
	public class NavPath
	{
		public Vector3 TargetPosition { get; set; }
		public List<Vector3> Points => new();
		public bool IsEmpty => Points.Count <= 1;

		public void Update( Vector3 from, Vector3 to )
		{
			var needsBuild = false;

			if ( !TargetPosition.IsNearlyEqual( to, 5 ) )
			{
				TargetPosition = to;
				needsBuild = true;
			}

			if ( needsBuild )
			{
				Points.Clear();
				NavMesh.BuildPath( from, to, Points );
			}

			if ( Points.Count <= 1 )
			{
				return;
			}

			var deltaToNext = from - Points[1];
			var delta = Points[1] - Points[0];
			var deltaNormal = delta.Normal;

			if ( deltaToNext.WithZ( 0 ).Length < 20 )
			{
				Points.RemoveAt( 0 );
				return;
			}

			if ( deltaToNext.Normal.Dot( deltaNormal ) >= 1.0f )
			{
				Points.RemoveAt( 0 );
			}
		}

		public float Distance( int point, Vector3 from )
		{
			if ( Points.Count <= point ) return float.MaxValue;

			return Points[point].WithZ( from.z ).Distance( from );
		}

		public Vector3 GetDirection( Vector3 position )
		{
			if ( Points.Count == 1 )
			{
				return (Points[0] - position).WithZ( 0 ).Normal;
			}

			return (Points[1] - position).WithZ( 0 ).Normal;
		}
	}
}
