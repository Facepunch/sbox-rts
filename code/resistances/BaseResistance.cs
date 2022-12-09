using Sandbox;

namespace Facepunch.RTS
{
	public abstract class BaseResistance
	{
		public uint NetworkId { get; set; }
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual Texture Icon => null;
		public virtual string DamageType => "generic";
	}
}
