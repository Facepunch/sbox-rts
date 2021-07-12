using Sandbox;

namespace Facepunch.RTS
{
	public abstract class BaseResistance
	{
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual Texture Icon => null;
		public virtual DamageFlags Flags => DamageFlags.Generic;
	}
}
