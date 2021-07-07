using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Abilities
{
	public abstract class BaseAbility
	{
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual string Description => "";
		public virtual AbilityTargetType TargetType => AbilityTargetType.Self;
		public virtual AbilityTargetTeam TargetTeam => AbilityTargetTeam.Self;
		public virtual Dictionary<ResourceType, int> Costs => new();
		public virtual Color Color => Color.White;
		public virtual Texture Icon => null;
		public virtual string Sound => "";
		public virtual float Cooldown => 10f;
		public virtual float MaxDistance => 0f;
		public virtual float AreaOfEffect => 0f;
		public virtual HashSet<string> Dependencies => new();

		public bool HasDependencies( Player player )
		{
			foreach ( var v in Dependencies )
			{
				var dependency = ItemManager.Find<BaseItem>( v );

				if ( dependency == null )
					throw new Exception( "[BaseAbility::HasDependencies] Unable to locate item by id: " + v );

				if ( !player.Dependencies.Contains( dependency.NetworkId ) )
					return false;
			}

			return true;
		}

		public virtual bool IsTargetValid( Player player, ISelectable target )
		{
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

		public virtual void Use( Player player, UseAbilityInfo info ) { }

		public virtual RequirementError CanUse( Player player )
		{
			if ( !player.CanAfford( this, out var resource ) )
			{
				return resource.ToRequirementError();
			}

			return RequirementError.Success;
		}
	}
}
