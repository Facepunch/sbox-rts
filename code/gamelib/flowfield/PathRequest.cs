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

		public bool IsValid()
		{
			return (FlowField != null);
		}
    }
}
