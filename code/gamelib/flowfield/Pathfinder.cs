using Gamelib.Math;
using System.Threading.Tasks;
using System.Threading;
using Sandbox;

namespace Gamelib.FlowField
{
	public class Pathfinder
	{
		public FlowField FlowField { get; private set; }

		public Pathfinder()
		{
			InitializeFlowField();
		}

		private void InitializeFlowField()
		{
			var realTimeNow = RealTime.Now;

			if ( FlowField == null )
			{
				FlowField = new FlowField( 40f, new Vector2i( 500, 500 ) );
				FlowField.CreateGrid();
			}

			var delta = RealTime.Now - realTimeNow;

			Log.Info( "Initialized Flow Field (" + (delta * 1000) + "ms)" );
		}

		public void Update( Vector3 target )
		{
			Task.Run( () => UpdateThread( target ) );
		}

		private Task UpdateThread( Vector3 target )
		{
			var realTimeNow = RealTime.Now;

			FlowField.CreateCostField();
			var node = FlowField.GetNodeFromWorld( target );
			FlowField.CreateIntegrationField( node );
			FlowField.CreateFlowField();

			var delta = RealTime.Now - realTimeNow;

			Log.Info( "Updated Flow Field (" + (delta * 1000) + "ms)" );

			return Task.CompletedTask;
		}
	}
}
