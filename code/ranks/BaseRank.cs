using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Ranks
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
	}
}
