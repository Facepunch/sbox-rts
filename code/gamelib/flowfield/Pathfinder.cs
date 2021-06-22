using Gamelib.Math;
using System;
using Sandbox;

namespace Gamelib.FlowField
{
	public class Pathfinder
	{
		public FlowField FlowField { get; private set; }

		private void InitializeFlowField()
		{
			//if ( FlowField != null ) return;

			FlowField = new FlowField( 10f, new Vector2i( 1000, 1000 ) );
			FlowField.CreateGrid();
			FlowField.CreateCostField();
		}

		public void Update( Vector3 target )
		{
			InitializeFlowField();

			var node = FlowField.GetNodeFromWorld( target );
			FlowField.CreateIntegrationField( node );
			FlowField.CreateFlowField();
		}
	}
}
