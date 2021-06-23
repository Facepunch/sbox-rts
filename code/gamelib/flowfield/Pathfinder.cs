using Gamelib.Math;
using System.Threading.Tasks;
using Gamelib.Extensions;
using System.Threading;
using Sandbox;
using System.Collections;
using System.Collections.Generic;

namespace Gamelib.FlowField
{
	public class Pathfinder
	{
		private Stack<FlowField> Pool { get; set; }

		public Pathfinder()
		{
			Pool = new();
			_ = Populate();
		}

		public FlowField Request( Vector3 from, Vector3 target )
		{
			var flowField = Pool.Pop();
			flowField.CurrentPortals = flowField.FindPath( from, target );
			return flowField;
		}

		public void Finish( FlowField flowField )
		{
			Pool.Push( flowField );
		}

		private async Task Populate()
		{
			for ( var i = 0; i < 1; i++ )
			{
				var flowField = new FlowField();
				flowField.CreateWorld( 10000, 1000, 100f );
				Pool.Push( flowField );

				await Task.Delay( 100 );
			}
		}

		/*
		public void Update( Vector3 from, Vector3 target )
		{
			Task.Run( () => UpdateThread( from, target ) );
		}

		private Task UpdateThread( Vector3 from, Vector3 target )
		{
			var realTimeNow = RealTime.Now;

			FlowField.FindPath( from, target );

			var delta = RealTime.Now - realTimeNow;

			Log.Info( "Updated Flow Field (" + (delta * 1000) + "ms)" );

			return Task.CompletedTask;
		}
		*/
	}
}
