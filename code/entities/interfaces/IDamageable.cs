using Sandbox;

namespace Facepunch.RTS
{
	public interface IDamageable
	{
		public void DoImpactEffects( Vector3 position, Vector3 normal );
		public void CreateDamageDecals( Vector3 position );
	}
}
