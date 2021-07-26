﻿using System.Collections.Generic;
using System.IO;
using Facepunch.RTS.Upgrades;
using Sandbox;

namespace Facepunch.RTS
{
	public interface ISelectable
	{
		public Dictionary<string, IStatus> Statuses { get; }
		public BBox WorldSpaceBounds { get; }
		public Vector3 LocalCenter { get; }
		public int NetworkIdent { get; }
		public string ItemId { get; }
		public Player Player { get; }
		public bool IsSelected { get; }
		public bool CanMultiSelect { get; }
		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public List<QueueItem> Queue { get; }
		public bool IsLocalPlayers { get; }
		public Vector3 Position { get; set; }
		public int GetAttackPriority();
		public bool HasStatus( string id );
		public bool IsInQueue( BaseItem item );
		public void QueueItem( BaseItem item );
		public BaseItem UnqueueItem( uint queueId );
		public void TakeDamage( DamageInfo info );
		public float GetDiameterXY( float scalar, bool smallestSide );
		public S ApplyStatus<S>( StatusData data ) where S : IStatus;
		public bool HasUpgrade( BaseUpgrade item );
		public bool HasUpgrade( uint id );
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
