﻿using Sandbox;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "unit_aircraft" )]
	public partial class AircraftEntity : UnitEntity
	{
		public RealTimeUntil ReturnToAirTime { get; private set; }

		private Particles DustParticles { get; set; }

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

		public override VerticalOffset GetVerticalOffset()
		{
			var groundHeight = Pathfinder.GetHeight( Position );
			var airHeight = groundHeight + Item.VerticalOffset;
			var lowHeight = groundHeight + 100f;

			float targetHeight;
			if ( ReturnToAirTime )
				targetHeight = airHeight;
			else
				targetHeight = lowHeight;

			return new VerticalOffset
			{
				Offset = targetHeight,
				Normal = Vector3.Up
			};
		}

		protected override void ServerTick()
		{
			var groundHeight = Pathfinder.GetHeight( Position );
			var airHeight = groundHeight + Item.VerticalOffset;
			var lowHeight = groundHeight + 100f;

			if ( !ReturnToAirTime )
			{
				if ( DustParticles == null )
				{
					DustParticles = Particles.Create( "particles/vehicle/helicopter_dust/helicopter_dust.vpcf" );
				}

				var difference = (airHeight - lowHeight);
				var fraction = 1f - ((1f / difference) * Position.z);

				DustParticles.SetPosition( 0, Position.WithZ( groundHeight ) );
				DustParticles.SetPosition( 1, new Vector3( 255f * fraction, 500f * fraction, 0f ) );
			}
			else if ( DustParticles != null )
			{
				DustParticles.Destroy();
				DustParticles = null;
			}

			base.ServerTick();
		}

		protected override void OnTargetChanged()
		{
			ReturnToAirTime = 0f;

			base.OnTargetChanged();
		}
	}
}
