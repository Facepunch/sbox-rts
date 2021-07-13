using Facepunch.RTS.Managers;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public abstract class BaseAbility
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual AbilityTargetType TargetType => AbilityTargetType.Self;
		public virtual AbilityTargetTeam TargetTeam => AbilityTargetTeam.Self;
		public virtual Dictionary<ResourceType, int> Costs => new();
		public virtual Color Color => Color.White;
		public virtual Texture Icon => null;
		public virtual bool LookAtTarget => true;
		public virtual float Duration => 0f;
		public virtual string Sound => "";
		public virtual float Cooldown => 10f;
		public virtual float MaxDistance => 0f;
		public virtual float AreaOfEffect => 0f;
		public virtual HashSet<string> Dependencies => new();

		public AbilityTargetInfo TargetInfo { get; set; }
		public RealTimeUntil NextUseTime { get; set; }
		public TimeSince LastUsedTime { get; set; }
		public ISelectable User { get; private set; }
		public string UniqueId { get; private set; }

		public virtual void Initialize( string uniqueId, ISelectable user )
		{
			UniqueId = uniqueId;
			User = user;
		}

		public float GetCooldownTimeLeft()
		{
			if ( NextUseTime ) return 0f;
			return NextUseTime.Relative;
		}

		public bool HasDependencies()
		{
			var player = User.Player;

			if ( !player.IsValid() )
			{
				return false;
			}

			foreach ( var v in Dependencies )
			{
				var dependency = Items.Find<BaseItem>( v );

				if ( dependency == null )
					throw new Exception( "[BaseAbility::HasDependencies] Unable to locate item by id: " + v );

				if ( !player.Dependencies.Contains( dependency.NetworkId ) )
					return false;
			}

			return true;
		}

		public virtual bool IsTargetValid( ISelectable target )
		{
			var player = User.Player;

			if ( !player.IsValid() )
			{
				return false;
			}

			if ( TargetType == AbilityTargetType.Self || TargetType == AbilityTargetType.None )
				return false;

			if ( TargetType == AbilityTargetType.Unit && target is not UnitEntity )
				return false;

			if ( TargetType == AbilityTargetType.Building && target is not BuildingEntity )
				return false;

			if ( TargetTeam == AbilityTargetTeam.Enemy && target.Player == player )
				return false;

			if ( TargetTeam == AbilityTargetTeam.Self && target.Player != player )
				return false;

			return true;
		}

		public virtual void Tick() { }

		public virtual void OnStarted() { }
		public virtual void OnFinished() { }
		public virtual void OnCancelled() { }

		public virtual RequirementError CanUse()
		{
			var player = User.Player;

			if ( !NextUseTime )
				return RequirementError.Cooldown;

			if ( !player.IsValid() )
				return RequirementError.InvalidPlayer;

			if ( !player.CanAfford( this, out var resource ) )
			{
				return resource.ToRequirementError();
			}

			return RequirementError.Success;
		}
	}
}
