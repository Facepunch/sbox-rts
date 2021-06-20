using System.Collections.Generic;

namespace Facepunch.RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem
	{
		public override Color Color => new Color( 0.8f, 0.8f, 0.8f );
		public virtual HashSet<string> Buildables => new();
		public virtual bool CanDepositResources => false;
		public virtual float MinLineOfSight => 200f;
		public virtual uint PopulationBoost => 0;
		public virtual uint MaxOccupants => 0;
		public virtual float MaxHealth => 100f;
		public virtual string Model => "";
		public virtual float AttackRange => 600f;
		public virtual string Weapon => "";
	}
}
