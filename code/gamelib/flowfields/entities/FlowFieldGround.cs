using Sandbox;

namespace Gamelib.FlowFields.Entities
{
	[Library( "flowfield_ground" )]
	public class FlowFieldGround : FuncBrush
	{
		public static BBox Bounds { get; private set; }
		public static bool Exists { get; private set; }

		public override void Spawn()
		{
			base.Spawn();

			CheckMinsMaxs();

			Tags.Add( "flowfield" );

			Transmit = TransmitType.Always;
			Exists = true;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Exists = true;
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

			Bounds = totalBounds;
		}
	}
}
