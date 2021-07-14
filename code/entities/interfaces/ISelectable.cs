using System.Collections.Generic;
using Sandbox;

namespace Facepunch.RTS
{
	public interface ISelectable
	{
		public Dictionary<string, BaseStatus> Statuses { get; }
		public BBox WorldSpaceBounds { get; }
		public int NetworkIdent { get; }
		public string ItemId { get; }
		public Player Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public int AttackPriority { get; }
		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public bool IsLocalPlayers { get; }
		public Vector3 Position { get; set; }
		public bool HasStatus( string id );
		public void TakeDamage( DamageInfo info );
		public float GetDiameterXY( float scalar, bool smallestSide );
		public BaseStatus ApplyStatus( string id );
		public void RemoveStatus( string id );
		public BaseAbility GetAbility( string id );
		public void StartAbility( BaseAbility ability, AbilityTargetInfo info );
		public void FinishAbility();
		public void CancelAbility();
		public bool IsUsingAbility();
		public void Assign( Player player, string itemId );
		public bool ShouldUpdateHud();
		public void UpdateHudComponents();
		public bool CanSelect();
		public void Select();
		public void Deselect();
	}
}
