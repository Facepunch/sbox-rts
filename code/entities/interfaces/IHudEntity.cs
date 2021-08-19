using Sandbox;

namespace Facepunch.RTS
{
	public interface IHudEntity
	{
		public Vector3 LocalCenter { get; }
		public Vector3 Position { get; }
		public bool ShouldUpdateHud();
		public void UpdateHudComponents();
	}
}
