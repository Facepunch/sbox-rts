using Gamelib.Network;
using Facepunch.RTS.Buildings;
using Sandbox;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelib.Extensions;
using Facepunch.RTS.Abilities;

namespace Facepunch.RTS
{
	public abstract partial class ItemEntity<T> : AnimEntity, ISelectable where T : BaseItem
	{
		public virtual bool CanMultiSelect => false;
		public virtual bool HasSelectionGlow => true;
		public virtual int AttackPriority => 0;

		public Dictionary<string, BaseAbility> Abilities { get; private set; }
		public BaseAbility UsingAbility { get; private set; }
		[Net, OnChangedCallback] public uint ItemId { get; set; }
		[Net] public Player Player { get; private set; }
		[Net] public float MaxHealth { get; set; }
		public EntityHudAnchor UI { get; private set; }

		public bool IsSelected => Tags.Has( "selected" );
		public bool IsLocalPlayers => Player.IsValid() && Player.IsLocalPawn;

		private T _itemCache;

		public T Item
		{
			get
			{
				if ( _itemCache == null )
					_itemCache = ItemManager.Find<T>( ItemId );
				return _itemCache;
			}
		}

		public ItemEntity()
		{
			Transmit = TransmitType.Always;
		}

		public BaseAbility GetAbility( string id )
		{
			if ( Abilities.TryGetValue( id, out var ability ) )
				return ability;

			return null;
		}

		public bool IsUsingAbility()
		{
			return (UsingAbility != null);
		}

		public virtual void StartAbility( BaseAbility ability, AbilityTargetInfo info )
		{
			CancelAbility();

			ability.LastUsedTime = 0;
			ability.NextUseTime = ability.Cooldown;
			ability.TargetInfo = info;

			ability.OnStarted();

			if ( IsServer )
			{
				ClientStartAbility( To.Single( Player ), ability.UniqueId, (Entity)info.Target, info.Origin );
			}

			UsingAbility = ability;

			if ( ability.Cooldown == 0f )
			{
				FinishAbility();
			}
		}

		public virtual void FinishAbility()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.OnFinished();
				UsingAbility = null;

				if ( IsServer )
				{
					ClientFinishAbility( To.Single( Player ) );
				}
			}
		}

		public virtual void CancelAbility()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.OnCancelled();
				UsingAbility = null;

				if ( IsServer )
				{
					ClientCancelAbility( To.Single( Player ) );
				}
			}
		}

		public bool IsEnemy( ISelectable other )
		{
			return (other.Player != Player);
		}

		public void Assign( Player player, T item )
		{
			Host.AssertServer();

			Owner = player;
			Player = player;
			ItemId = item.NetworkId;

			OnItemChanged( item );
			OnPlayerAssigned( player );
		}

		public float GetDiameterXY( float scalar = 1f, bool smallestSide = false )
		{
			return EntityExtension.GetDiameterXY( this, scalar, smallestSide );
		}

		public void Assign( Player player, string itemId )
		{
			Host.AssertServer();

			var item = ItemManager.Find<T>( itemId );

			Assign( player, item );
		}

		public virtual void Select()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Add( this );
				Tags.Add( "selected" );
			}
		}

		public virtual void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
				Tags.Remove( "selected" );
			}
		}

		public virtual bool CanSelect()
		{
			return true;
		}

		public override void ClientSpawn()
		{
			UI = EntityHud.Instance.Create( this );

			AddHudComponents();

			base.ClientSpawn();
		}

		[Event.Frame]
		protected void UpdateHudAnchor()
		{
			if ( IsClient && ShouldUpdateHud() )
			{
				UpdateHudComponents();
				UI.UpdatePosition();
			}
		}

		[Event.Tick]
		protected virtual void Tick()
		{
			if ( UsingAbility != null )
			{
				UsingAbility.Tick();
			}
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			var ability = UsingAbility;
			if ( ability == null ) return;

			if ( ability.LastUsedTime >= ability.Duration )
			{
				FinishAbility();
			}
		}

		[Event.Tick.Client]
		protected virtual void ClientTick() { }

		protected virtual void OnItemIdChanged()
		{
			CreateAbilities();
		}

		protected virtual void AddHudComponents() { }

		protected virtual void UpdateHudComponents() { }

		protected virtual bool ShouldUpdateHud()
		{
			return EnableDrawing && UI.IsVisibleSelf;
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				CancelAbility();
				Deselect();
			}

			if ( IsClient ) UI.Delete();

			base.OnDestroy();
		}

		protected override void OnTagAdded( string tag )
		{
			if ( HasSelectionGlow && IsLocalPlayers && tag == "selected" )
			{
				GlowActive = true;
				GlowState = GlowStates.GlowStateOn;
				GlowColor = Player.TeamColor.WithAlpha( 0.5f );
			}

			base.OnTagAdded( tag );
		}

		protected override void OnTagRemoved( string tag )
		{
			if ( HasSelectionGlow && IsLocalPlayers && tag == "selected" )
			{
				GlowActive = false;
			}

			base.OnTagRemoved( tag );
		}

		protected virtual void OnPlayerAssigned( Player player) { }

		protected virtual void OnItemChanged( T item )
		{
			CreateAbilities();
		}

		protected virtual void CreateAbilities()
		{
			Abilities = new();

			foreach ( var id in Item.Abilities )
			{
				var ability = AbilityManager.Create( id );
				ability.Initialize( id, this );
				Abilities[id] = ability;
			}
		}

		[ClientRpc]
		private void ClientStartAbility( string id, Entity target, Vector3 origin )
		{
			StartAbility( GetAbility( id ), new AbilityTargetInfo()
			{
				Target = target as ISelectable,
				Origin = origin
			} );
		}

		[ClientRpc]
		private void ClientFinishAbility()
		{
			FinishAbility();
		}

		[ClientRpc]
		private void ClientCancelAbility()
		{
			CancelAbility();
		}
	}
}

