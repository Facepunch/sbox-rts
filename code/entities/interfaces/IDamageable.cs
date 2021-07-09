using Sandbox;

namespace Facepunch.RTS
{
	public interface IDamageable
	{
		public Player Player { get; }
		public void DoImpactEffects( TraceResult trace );
		public void CreateDamageDecals( Vector3 position );
	}
}
