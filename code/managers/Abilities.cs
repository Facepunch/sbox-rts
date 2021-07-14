﻿using System.Collections.Generic;
using Gamelib.Extensions;
using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS.Managers
{
	public static partial class Abilities
	{
		private static BaseAbility SelectingTargetFor { get; set; }

		public static bool IsSelectingTarget()
		{
			return (SelectingTargetFor != null);
		}

		[ServerCmd]
		public static void UseOnTarget( int entityId, string abilityId, int targetId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is not ISelectable selectable )
				return;

			if ( Entity.FindByIndex( targetId ) is not ISelectable target )
				return;

			var ability = selectable.GetAbility( abilityId );
			if ( ability == null ) return;

			if ( !ability.IsTargetValid( target ) )
				return;

			var item = Items.Find<BaseItem>( selectable.ItemId );

			if ( selectable.Player == caller && item.Abilities.Contains( abilityId ) )
			{
				var position = target.Position;

				if ( selectable.Position.Distance( position ) > ability.MaxDistance )
					return;

				if ( ability.CanUse() == RequirementError.Success )
				{
					if ( !ability.CanTarget( target ) )
						return;

					ResourceHint.Send( caller, 2f, position, ability.Costs, Color.Green );

					caller.TakeResources( ability );

					selectable.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = target,
						Origin = position
					} );
				}
			}
		}

		[ServerCmd]
		public static void UseAtLocation( int entityId, string abilityId, string origin )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is not ISelectable selectable )
				return;


			var ability = selectable.GetAbility( abilityId );
			if ( ability == null ) return;

			if ( ability.TargetType != AbilityTargetType.None )
				return;

			var item = Items.Find<BaseItem>( selectable.ItemId );

			if ( selectable.Player == caller && item.Abilities.Contains( abilityId ) )
			{
				var position = origin.ToVector3();

				if ( selectable.Position.Distance( position ) > ability.MaxDistance )
					return;

				if ( ability.CanUse() == RequirementError.Success )
				{
					ResourceHint.Send( caller, 2f, position, ability.Costs, Color.Green );

					caller.TakeResources( ability );

					selectable.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = null,
						Origin = position
					} );
				}
			}
		}

		[ServerCmd]
		public static void UseOnSelf( int entityId, string abilityId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is not ISelectable selectable )
				return;

			var ability = selectable.GetAbility( abilityId );
			if ( ability == null ) return;

			if ( ability.TargetType != AbilityTargetType.Self )
				return;

			var item = Items.Find<BaseItem>( selectable.ItemId );

			if ( selectable.Player == caller && item.Abilities.Contains( abilityId ) )
			{
				if ( ability.CanUse() == RequirementError.Success )
				{
					ResourceHint.Send( caller, 2f, selectable.Position, ability.Costs, Color.Green );

					caller.TakeResources( ability );

					selectable.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = selectable,
						Origin = selectable.Position
					} );
				}
			}
		}

		public static BaseAbility Create( string id )
		{
			return Library.Create<BaseAbility>( id );
		}

		public static void SelectTarget( BaseAbility ability )
		{
			SelectingTargetFor = ability;
		}

		[Event.Tick.Client]
		private static void ClientTick()
		{
			if ( SelectingTargetFor == null ) return;

			var ability = SelectingTargetFor;
			var player = Local.Pawn as Player;

			if ( ability.User == null || ability.User.Health == 0f )
			{
				SelectingTargetFor = null;
				return;
			}

			var cursorOrigin = Input.Cursor.Origin;
			var cursorAim = Input.Cursor.Direction;

			if ( ability.TargetType == AbilityTargetType.None )
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim )
					.WithTag( "flowfield" )
					.Run();

				if ( ability.User.Position.Distance( trace.EndPos ) < ability.MaxDistance )
				{
					DebugOverlay.Sphere( trace.EndPos, ability.AreaOfEffect, Color.Green );

					if ( Input.Released( InputButton.Attack1 ) )
					{
						SelectingTargetFor = null;
						UseAtLocation( ability.User.NetworkIdent, ability.UniqueId, trace.EndPos.ToCSV() );
					}
				}
				else
				{
					DebugOverlay.Sphere( trace.EndPos, ability.AreaOfEffect, Color.Red );
				}
			}
			else
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim ).Run();

				if ( trace.Entity is ISelectable target && target.Position.Distance( ability.User.Position ) < ability.MaxDistance )
				{
					if ( ability.IsTargetValid( target ) )
					{
						if ( Input.Released( InputButton.Attack1 ) )
						{
							SelectingTargetFor = null;
							UseOnTarget( ability.User.NetworkIdent, ability.UniqueId, target.NetworkIdent );
						}

						DebugOverlay.Sphere( trace.EndPos, ability.AreaOfEffect, Color.Green );
					}
				}
				else
				{
					DebugOverlay.Sphere( trace.EndPos, ability.AreaOfEffect, Color.Red );
				}
			}

			if ( Input.Released( InputButton.Attack2 ) )
			{
				SelectingTargetFor = null;
			}
		}
	}
}
