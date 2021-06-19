using System.Collections.Generic;

namespace RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem
	{
		public override Color Color => Color.Magenta;
		public virtual HashSet<string> Buildables => new();
		public virtual bool CanDepositResources => false;
		public virtual float MinLineOfSight => 200f;
		public virtual uint PopulationBoost => 0;
		public virtual float MaxHealth => 100f;
		public virtual string Model => "";
	}
}
