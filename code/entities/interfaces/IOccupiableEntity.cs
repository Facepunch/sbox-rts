using Sandbox;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Facepunch.RTS
{
	public interface IOccupiableEntity
	{
		public IOccupiableItem OccupiableItem { get; }
		public bool CanOccupyUnits { get; }
		public int NetworkIdent { get; }
		public Player Player { get; }

		public IList<UnitEntity> GetOccupantsList();
		public Vector3? GetVacatePosition( UnitEntity unit );
		public void DamageOccupants( DamageInfo info );
		public bool OccupyUnit( UnitEntity unit );
		public void EvictUnit( UnitEntity unit );
		public void EvictAll();
	}
}
