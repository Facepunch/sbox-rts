using System.Collections.Generic;

namespace Gamelib.FlowFields
{
    public class Pathfinding
    {
		private Pathfinder _pathfinder;
		private FlowField _flowField;

        public void Initialize()
		{
			_pathfinder = new Pathfinder();
			_pathfinder.Init();
			_pathfinder.UpdateCollisions();

			_flowField = new FlowField( _pathfinder );
		}

		public FlowField FindPath( Vector3 from, Vector3 to )
		{
			var gridPosition = _pathfinder.CreateWorldPosition( to );
			var worldIndex = new List<int> { gridPosition.WorldIndex };

			_flowField.ResetDestination();
			_flowField.CreateDestinationGateways( worldIndex );

			return _flowField;
		}

		public void Update()
		{
			_pathfinder.Update();
		}
    }
}
