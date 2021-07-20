using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public interface IOccupiableEntity
	{
		public IOccupiableItem OccupiableItem { get; }
		public Vector3 Position { get; }
		public bool CanOccupyUnits { get; }
		public int NetworkIdent { get; }
		public Player Player { get; }
		public bool CanOccupantsAttack();
		public float GetMaxVerticalRange();
		public float GetAttackRadius();
		public bool IsInRange( Entity entity, float radius );
		public IList<UnitEntity> GetOccupantsList();
		public Vector3? GetVacatePosition( UnitEntity unit );
		public void DamageOccupants( DamageInfo info );
		public Transform? GetAttackAttachment( Entity target );
		public Transform? GetAttachment( string name, bool worldspace );
		public bool OccupyUnit( UnitEntity unit );
		public void EvictUnit( UnitEntity unit );
		public void EvictAll();
	}
}
