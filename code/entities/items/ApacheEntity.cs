using Sandbox;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "unit_apache" )]
	public partial class ApacheEntity : AircraftEntity
	{
		public Vector3 TargetDirection { get; private set; }

		public override bool CanOccupantsAttack()
		{
			var occupant = Occupants.FirstOrDefault();
			if ( !occupant.IsValid() ) return false;

			var target = occupant.GetTargetEntity();
			if ( !target.IsValid() ) return false;

			var goalDirection = (target.Position - Position).Normal;

			if ( TargetDirection.Distance( goalDirection ) > 2f )
				return false;

			return true;
		}

		protected override void ServerTick()
		{
			var occupant = Occupants.FirstOrDefault();

			if ( occupant.IsValid() )
			{
				var target = occupant.GetTargetEntity();

				occupant.Position = Position;

				if ( target.IsValid() )
				{
					TargetDirection = TargetDirection.LerpTo( (target.Position - Position).Normal, Time.Delta * 10f );
					SetAnimVector( "target", Transform.NormalToLocal( TargetDirection ) );
				}
			}

			base.ServerTick();
		}
	}
}
