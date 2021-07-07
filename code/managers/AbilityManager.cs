using System.Collections.Generic;
using Facepunch.RTS.Abilities;
using Gamelib.Extensions;
using Gamelib.FlowFields;
using Sandbox;

namespace Facepunch.RTS
{
	public static partial class AbilityManager
	{
		public struct LocationSelector
		{
			public ISelectable User;
			public BaseAbility Ability;
		}

		public static Dictionary<string, BaseAbility> Table { get; private set; }
		public static List<BaseAbility> List { get; private set; }

		public static LocationSelector? Selector { get; private set; }

		public static T Find<T>( string id ) where T : BaseAbility
		{
			if ( Table.TryGetValue( id, out var item ) )
				return (item as T);

			return null;
		}

		[ServerCmd]
		public static void UseOnTarget( int entityId, string abilityId, int targetId )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is not UnitEntity unit )
				return;

			if ( Entity.FindByIndex( targetId ) is not ISelectable target )
				return;

			var ability = Find<BaseAbility>( abilityId );
			if ( ability == null ) return;

			if ( !ability.IsTargetValid( caller, target ) )
				return;

			if ( unit.Player == caller && unit.Item.Abilities.Contains( abilityId ) )
			{
				var position = target.Position;

				if ( unit.Position.Distance( position ) > ability.MaxDistance )
					return;

				if ( ability.CanUse( caller ) == RequirementError.Success )
				{
					caller.TakeResources( ability );

					ability.Use( caller, new UseAbilityInfo()
					{
						user = unit,
						target = target,
						origin = position
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

			if ( Entity.FindByIndex( entityId ) is not UnitEntity unit )
				return;

			var ability = Find<BaseAbility>( abilityId );
			if ( ability == null ) return;

			if ( ability.TargetType != AbilityTargetType.None )
				return;

			if ( unit.Player == caller && unit.Item.Abilities.Contains( abilityId ) )
			{
				var position = origin.ToVector3();

				if ( unit.Position.Distance( position ) > ability.MaxDistance )
					return;

				if ( ability.CanUse( caller ) == RequirementError.Success )
				{
					caller.TakeResources( ability );

					ability.Use( caller, new UseAbilityInfo()
					{
						user = unit,
						target = null,
						origin = position
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

			if ( Entity.FindByIndex( entityId ) is not UnitEntity unit )
				return;

			var ability = Find<BaseAbility>( abilityId );
			if ( ability == null ) return;

			if ( ability.TargetType != AbilityTargetType.Self )
				return;

			if ( unit.Player == caller && unit.Item.Abilities.Contains( abilityId ) )
			{
				if ( ability.CanUse( caller ) == RequirementError.Success )
				{
					caller.TakeResources( ability );

					ability.Use( caller, new UseAbilityInfo() {
						user = unit,
						target = unit,
						origin = unit.Position
					} );
				}
			}
		}

		public static void Initialize()
		{
			BuildTable();
		}

		private static void BuildTable()
		{
			Table = new();
			List = new();

			var list = new List<BaseAbility>();

			foreach ( var type in Library.GetAll<BaseAbility>() )
			{
				var ability = Library.Create<BaseAbility>( type );
				list.Add( ability );
			}

			for ( var i = 0; i < list.Count; i++ )
			{
				var ability = list[i];

				Table.Add( ability.UniqueId, ability );
				List.Add( ability );

				Log.Info( $"Adding {ability.UniqueId} to the available abilities (id = {i})" );
			}

			List.Sort();
		}

		public static void SelectLocation( ISelectable user, BaseAbility ability )
		{
			Selector = new LocationSelector()
			{
				User = user,
				Ability = ability
			};
		}

		[Event.Tick.Client]
		private static void ClientTick()
		{
			if ( !Selector.HasValue ) return;

			var selector = Selector.Value;
			var player = Local.Pawn as Player;

			if ( selector.User == null || selector.User.Health == 0f )
			{
				Selector = null;
				return;
			}

			var cursorOrigin = Input.Cursor.Origin;
			var cursorAim = Input.Cursor.Direction;

			if ( selector.Ability.TargetType == AbilityTargetType.None )
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim )
					.WithTag( "flowfield" )
					.Run();

				if ( selector.User.Position.Distance( trace.EndPos ) < selector.Ability.MaxDistance )
				{
					DebugOverlay.Sphere( trace.EndPos, selector.Ability.AreaOfEffect, Color.Green );

					if ( Input.Released( InputButton.Attack1 ) )
					{
						Selector = null;
						UseAtLocation( selector.User.NetworkIdent, selector.Ability.UniqueId, trace.EndPos.ToCSV() );
					}
				}
				else
				{
					DebugOverlay.Sphere( trace.EndPos, selector.Ability.AreaOfEffect, Color.Red );
				}
			}
			else
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim ).Run();

				if ( trace.Entity is ISelectable target && target.Position.Distance( selector.User.Position ) < selector.Ability.MaxDistance )
				{
					if ( selector.Ability.IsTargetValid( player, target ) )
					{
						if ( Input.Released( InputButton.Attack1 ) )
						{
							Selector = null;
							UseOnTarget( selector.User.NetworkIdent, selector.Ability.UniqueId, target.NetworkIdent );
						}

						DebugOverlay.Sphere( trace.EndPos, selector.Ability.AreaOfEffect, Color.Green );
					}
				}
				else
				{
					DebugOverlay.Sphere( trace.EndPos, selector.Ability.AreaOfEffect, Color.Red );
				}
			}

			if ( Input.Released( InputButton.Attack2 ) )
			{
				Selector = null;
			}
		}
	}
}
