using System.Collections.Generic;

namespace RTS.Buildings
{
    public abstract class BaseBuilding : BaseItem
	{
		public virtual List<string> Buildables => new();
		public virtual float MaxHealth => 100f;
		public virtual string Model => "";
	}
}
