using System.Collections.Generic;

namespace RTS.Units
{
    public abstract class BaseUnit : BaseItem
	{
		public virtual float MaxHealth => 100f;
		public virtual string Model => "models/citizen/citizen.vmdl";
		public virtual HashSet<string> Clothing => new();
		public virtual bool CanConstruct => false;
		public virtual HashSet<ResourceType> Gatherables => new();
		public virtual float Speed => 200f;
		public virtual float LineOfSight => 600f;
		public virtual float AttackRange => 600f;
		public virtual float InteractRange => 10f;
		public virtual bool UseRenderColor => false;
		public virtual string Weapon => "";
		public virtual HashSet<string> Buildables => new();
	}
}
