using Sandbox;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "unit_aircraft" )]
	public partial class AircraftEntity : UnitEntity
	{
		public RealTimeUntil ReturnToAirTime { get; private set; }

		public override bool OccupyUnit( UnitEntity unit )
		{
			var groundHeight = Pathfinder.GetHeight( Position );

			if ( Position.z <= groundHeight + 150f )
			{
				if ( base.OccupyUnit( unit ) )
				{
					ReturnToAirTime = 0f;
					return true;
				}
			}
			else
			{
				ReturnToAirTime = 1f;
			}

			return false;
		}

		protected override void AlignToGround()
		{
			var targetHeight = Pathfinder.GetHeight( Position );

			if ( ReturnToAirTime )
				targetHeight += Item.VerticalOffset;
			else
				targetHeight += 100f;

			Position = Position.LerpTo( Position.WithZ( targetHeight ), Time.Delta );
		}

		protected override void OnTargetChanged()
		{
			ReturnToAirTime = 0f;

			base.OnTargetChanged();
		}
	}
}
