using Sandbox;

namespace Facepunch.RTS
{
	public interface IDamageable
	{
		public void DoImpactEffects( TraceResult trace );
		public void CreateDamageDecals( Vector3 position );
	}
}
