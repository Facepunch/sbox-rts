using Sandbox;
using System;

namespace Gamelib.FlowFields.Entities
{

	/// <summary>
	/// The main component needed for the flowfield to work, basically works as a nav mesh. Must be placed over all the playable space.
	/// </summary>

	[Library( "flowfield_ground" )]
	[Hammer.AutoApplyMaterial("materials/rts/hammer/flowfield_ground.vmat")]
	[Hammer.EntityTool( "FlowField Ground", "FlowField" )]
	[Hammer.Solid]
	public class FlowFieldGround : BaseTrigger
	{
		public static event Action OnUpdated;
		public static BBox Bounds { get; private set; }
		public static bool Exists { get; private set; }

		public override void Spawn()
		{
			base.Spawn();

			CheckMinsMaxs();

			EnableAllCollisions = false;
			EnableDrawing = false;
			Transmit = TransmitType.Always;
			Exists = true;

			OnUpdated?.Invoke();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			CheckMinsMaxs();
			Exists = true;

			OnUpdated?.Invoke();
		}

		private void CheckMinsMaxs()
		{
			var totalBounds = Bounds;
			var groundMins = WorldSpaceBounds.Mins;
			var groundMaxs = WorldSpaceBounds.Maxs;

			if ( groundMins.x < totalBounds.Mins.x )
				totalBounds.Mins.x = groundMins.x;

			if ( groundMins.y < totalBounds.Mins.y )
				totalBounds.Mins.y = groundMins.y;

			if ( groundMaxs.x > totalBounds.Maxs.x )
				totalBounds.Maxs.x = groundMaxs.x;

			if ( groundMaxs.y > totalBounds.Maxs.y )
				totalBounds.Maxs.y = groundMaxs.y;

			totalBounds.Maxs.z = groundMaxs.z;

			Bounds = totalBounds;
		}
	}
}
