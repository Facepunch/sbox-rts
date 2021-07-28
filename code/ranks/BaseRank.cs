using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public abstract class BaseRank : IComparer<BaseRank>, IComparable<BaseRank>
	{
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual Texture Icon => null;
		public virtual int Kills => 0;
		public virtual int DamageModifier => 0;

		public int Compare( BaseRank x, BaseRank y )
		{
			return x.Kills.CompareTo( y.Kills );
		}

		public int CompareTo( BaseRank other )
		{
			return Kills.CompareTo( other.Kills );
		}

		public virtual void OnGiven( UnitEntity unit )
		{
			unit.Modifiers.Damage += DamageModifier;
		}

		public virtual void OnTaken( UnitEntity unit )
		{
			unit.Modifiers.Damage -= DamageModifier;
		}
	}
}
