using Sandbox;

namespace Facepunch.RTS
{
    public abstract class ItemComponent
	{
		public virtual DamageInfo TakeDamage( DamageInfo info )
		{
			return info;
		}
	}
}
