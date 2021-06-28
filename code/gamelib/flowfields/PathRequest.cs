using Gamelib.FlowFields.Grid;
using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public class PathRequest
	{
		public FlowField FlowField;

		public Vector3 GetDirection( Vector3 position )
		{
			if ( !IsValid() || !FlowField.Ready( position ) )
				return Vector3.Zero;

			return FlowField.GetDirection( position );
		}

		public bool IsDestination( Vector3 position )
		{
			var indicies = FlowField.DestinationIndexes;
			var pathfinder = FlowField.Pathfinder;
			var worldPosition = pathfinder.CreateWorldPosition( position );
			return indicies.Contains( worldPosition.WorldIndex );
		}

		public bool IsValid()
		{
			return (FlowField != null);
		}
    }
}
