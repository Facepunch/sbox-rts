using Sandbox;

namespace Facepunch.RTS
{
	public interface IDamageable
	{
		public Player Player { get; }
		public void DoImpactEffects( Vector3 position, Vector3 normal );
		public void CreateDamageDecals( Vector3 position );
	}
}
