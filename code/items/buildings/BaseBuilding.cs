using System.Collections.Generic;

namespace RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem
	{
		public virtual HashSet<string> Buildables => new();
		public virtual bool CanDepositResources => false;
		public virtual float MinLineOfSight => 200f;
		public virtual float MaxHealth => 100f;
		public virtual string Model => "";
	}
}
