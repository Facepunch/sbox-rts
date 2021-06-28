using Facepunch.RTS;
using Gamelib.FlowFields;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class MoveGroup
	{
		public HashSet<UnitEntity> FinishedUnits { get; private set; }
		public List<UnitEntity> Units { get; private set; }
		public PathRequest PathRequest { get; private set; }

		public MoveGroup( List<UnitEntity> units, Vector3 destination )
		{
			FinishedUnits = new();
			PathRequest = RTS.Path.Request( destination );
			Units = units;
		}

		public MoveGroup( List<UnitEntity> units, List<Vector3> destinations )
		{
			FinishedUnits = new();
			PathRequest = RTS.Path.Request( destinations );
			Units = units;
		}

		public void Finish( UnitEntity unit )
		{
			if ( !IsValid() ) return;

			FinishedUnits.Add( unit );

			if ( FinishedUnits.Count == Units.Count )
			{
				Dispose();
			}
		}

		public void Remove( UnitEntity unit )
		{
			if ( !IsValid() ) return;

			Units.Remove( unit );

			if ( Units.Count == 0 )
			{
				Dispose();
			}
		}

		public Vector3 GetDirection( Vector3 position )
		{
			if ( IsValid() ) return PathRequest.GetDirection( position );
			return Vector3.Zero;
		}

		public bool IsDestination( UnitEntity unit, Vector3 position )
		{
			if ( !IsValid() || FinishedUnits.Contains( unit ) )
				return true;

			if ( PathRequest.IsDestination( position ) )
				return true;

			for ( int i = 0; i < Units.Count; i++ )
			{
				var other = Units[i];

				if ( other.MoveGroup == this && FinishedUnits.Contains( other ) )
				{
					var distance = unit.Position.Distance( other.Position );

					if ( distance <= (unit.FlockSettings.Radius * 2f) )
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool IsValid()
		{
			return (PathRequest != null && Units != null);
		}

		public void Dispose()
		{
			if ( PathRequest != null && PathRequest.IsValid() )
			{
				PathRequest = null;

				for ( int i = 0; i < Units.Count; i++ )
				{
					var unit = Units[i];

					if ( unit.MoveGroup == this )
					{
						unit.ClearTarget();
					}
				}

				RTS.Path.Complete( PathRequest );
				Units = null;
			}
		}
	}
}
