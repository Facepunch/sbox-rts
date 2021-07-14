using Sandbox;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "building_tunnel" )]
	public partial class TunnelEntity : BuildingEntity
	{
		[Net] public TunnelEntity Connection { get; set; }

		public void ConnectTo( TunnelEntity other )
		{
			other.Connection = this;
			Connection = other;
		}

		public override Vector3? GetVacatePosition( UnitEntity unit )
		{
			if ( Connection.IsValid() )
				return Connection.GetFreePosition( unit, 1.5f );
			else
				return null;
		}

		protected override void OnOccupied( UnitEntity unit )
		{
			EvictUnit( unit );
			base.OnOccupied( unit );
		}
	}
}
