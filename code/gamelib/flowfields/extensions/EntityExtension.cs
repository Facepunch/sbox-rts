using System;
using System.Collections.Generic;
using Gamelib.Extensions;
using Sandbox;

namespace Gamelib.FlowFields.Extensions
{
	public static class EntityExtension
	{
		public static List<Vector3> GetDestinations( this ModelEntity self, Pathfinder pathfinder, bool sortByDistance = false )
		{
			var collisionSize = pathfinder.CollisionSize;
			var nodeSize = pathfinder.NodeSize;

			// Round up the radius to the nearest node size.
			var radius = MathF.Ceiling( (self.GetDiameterXY( 0.6f ) + collisionSize / 2f) / nodeSize ) * nodeSize;
			var locations = new List<Vector3>();

			pathfinder.GetGridPositions( self.Position, radius, locations, true );

			if ( sortByDistance )
			{
				locations.Sort( ( a, b ) => a.Distance( self.Position ).CompareTo( b.Distance( self.Position ) ) );
			}

			return locations;
		}
	}
}
