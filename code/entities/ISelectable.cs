using Sandbox;

namespace Facepunch.RTS
{
	public interface ISelectable
	{
		public int NetworkIdent { get; }
		public Player Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public Vector3 Position { get; set; }
		public bool CanSelect();
		public void Select();
		public void Deselect();
	}
}
