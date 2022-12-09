using System.Collections.Generic;
using Facepunch.RTS.Upgrades;
using Sandbox;
using Sandbox.Internal;

namespace Facepunch.RTS
{
	public interface ISelectable
	{
		public Dictionary<string, BaseAbility> Abilities { get; }
		public Dictionary<string, IStatus> Statuses { get; }
		public BBox WorldSpaceBounds { get; }
		public Vector3 LocalCenter { get; }
		public uint ItemNetworkId { get; }
		public int NetworkIdent { get; }
		public string ItemId { get; }
		public EntityTags Tags { get; }
		public RTSPlayer Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public List<QueueItem> Queue { get; }
		public bool IsLocalPlayers { get; }
		public bool IsLocalTeamGroup { get; }
		public Vector3 Position { get; set; }
		public BaseItem GetBaseItem();
		public int GetAttackPriority();
		public bool HasStatus( string id );
		public bool IsInQueue( BaseItem item );
		public void QueueItem( BaseItem item );
		public bool IsSameTeamGroup( ISelectable other );
		public BaseItem UnqueueItem( uint queueId );
		public bool HasComponent<C>() where C : ItemComponent;
		public C GetComponent<C>() where C : ItemComponent;
		public void RemoveComponent<C>() where C : ItemComponent;
		public C AddComponent<C>() where C : ItemComponent;
		public void TakeDamage( DamageInfo info );
		public float GetDiameterXY( float scalar, bool smallestSide );
		public S ApplyStatus<S>( StatusData data ) where S : IStatus;
		public bool HasStatus<S>() where S : IStatus;
		public IEnumerable<BaseUpgrade> GetUpgrades();
		public bool HasUpgrade( BaseUpgrade item );
		public bool HasUpgrade( uint id );
		public void RemoveStatus( string id );
		public BaseAbility GetAbility( string id );
		public void StartAbility( BaseAbility ability, AbilityTargetInfo info );
		public void FinishAbility();
		public void CancelAbility();
		public bool IsUsingAbility();
		public void Assign( RTSPlayer player, string itemId );
		public bool ShouldUpdateHud();
		public void UpdateHudComponents();
		public bool CanBeAttacked();
		public bool CanSelect();
		public void CancelAction();
		public void Select();
		public void Deselect();
	}
}
