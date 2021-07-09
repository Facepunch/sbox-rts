using Facepunch.RTS.Abilities;

namespace Facepunch.RTS
{
	public interface ISelectable
	{
		public int NetworkIdent { get; }
		public Player Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public int AttackPriority { get; }
		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public bool IsLocalPlayers { get; }
		public Vector3 Position { get; set; }
		public BaseAbility GetAbility( string id );
		public void StartAbility( BaseAbility ability, AbilityTargetInfo info );
		public void FinishAbility();
		public void CancelAbility();
		public bool IsUsingAbility();
		public bool CanSelect();
		public void Select();
		public void Deselect();
	}
}
