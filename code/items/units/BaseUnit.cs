using System.Collections.Generic;

namespace RTS.Units
{
    public abstract class BaseUnit : BaseItem
	{
		public virtual float MaxHealth => 100f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual List<string> Clothing => new();
		public virtual float Speed => 200f;
		public virtual float Range => 600f;
		public virtual string Weapon => "";
	}
}
