using System;
using System.Linq;
using Gamelib.Extensions;
using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Abilities
	{
		private static BaseAbility SelectingTargetFor { get; set; }
		private static AbilityCircle TargetCircle { get; set; }

		public static bool IsSelectingTarget()
		{
			return (SelectingTargetFor != null);
		}

		[ConCmd.Server]
		public static void UseOnTarget( int entityId, string abilityId, int targetId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

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

					ResourceHint.Send( caller, 2f, position, ability.Costs, Color.Red );

					caller.TakeResources( ability );

					selectable.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = target,
						Origin = position
					} );
				}
			}
		}

		[ConCmd.Server]
		public static void UseAtLocation( int entityId, string abilityId, string origin )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

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

				if ( !ability.IsLocationValid( position ) )
					return;

				if ( ability.CanUse() == RequirementError.Success )
				{
					ResourceHint.Send( caller, 2f, position, ability.Costs, Color.Red );

					caller.TakeResources( ability );

					selectable.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = null,
						Origin = position
					} );
				}
			}
		}

		[ConCmd.Server]
		public static void UseOnSelf( int entityId, string abilityId )
		{
			var caller = ConsoleSystem.Caller.Pawn as RTSPlayer;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			if ( Entity.FindByIndex( entityId ) is not ISelectable selectable )
				return;

			if ( selectable.Player != caller )
				return;

			var selectablesOfType = caller.GetAllSelected().Where( s => s.ItemId == selectable.ItemId );

			foreach ( var item in selectablesOfType )
			{
				var ability = item.GetAbility( abilityId );
				if ( ability == null ) continue;

				if ( ability.TargetType != AbilityTargetType.Self )
					continue;

				if ( ability.CanUse() == RequirementError.Success )
				{
					ResourceHint.Send( caller, 2f, item.Position, ability.Costs, Color.Red );

					caller.TakeResources( ability );

					item.StartAbility( ability, new AbilityTargetInfo()
					{
						Target = item,
						Origin = item.Position
					} );
				}
			}
		}

		public static BaseAbility Create( string id )
		{
			return TypeLibrary.Create<BaseAbility>( id );
		}

		public static void SelectTarget( BaseAbility ability )
		{
			StopSelectingTarget();

			SelectingTargetFor = ability;

			TargetCircle = new AbilityCircle
			{
				EffectSize = ability.AreaOfEffectRadius,
				TargetSize = MathF.Max( ability.AreaOfEffectRadius * 0.8f, 100f ),
				EffectColor = Color.Orange
			};
		}

		public static void StopSelectingTarget()
		{
			SelectingTargetFor = null;

			if ( TargetCircle.IsValid() )
			{
				TargetCircle.Delete();
				TargetCircle = null;
			}
		}

		[Event.Tick.Client]
		private static void ClientTick()
		{
			if ( SelectingTargetFor == null ) return;

			var ability = SelectingTargetFor;

			if ( ability.User == null || ability.User.Health == 0f )
			{
				SelectingTargetFor = null;
				return;
			}

			if ( Game.LocalPawn is not RTSPlayer player )
				return;

			var cursorOrigin = player.CursorOrigin;
			var cursorAim = player.CursorDirection;

			if ( ability.TargetType == AbilityTargetType.None )
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim )
					.WorldOnly()
					.Run();

				TargetCircle.Position = trace.EndPosition;

				if ( ability.User.Position.Distance( trace.EndPosition ) < ability.MaxDistance )
				{
					if ( ability.IsLocationValid( trace.EndPosition ) )
					{
						TargetCircle.TargetColor = Color.Green;

						if ( Input.Down( InputButton.PrimaryAttack ) )
						{
							StopSelectingTarget();
							UseAtLocation( ability.User.NetworkIdent, ability.UniqueId, trace.EndPosition.ToCSV() );
							return;
						}
					}
					else
					{
						TargetCircle.TargetColor = Color.Red;
					}
				}
				else
				{
					TargetCircle.TargetColor = Color.Red;
				}
			}
			else
			{
				var trace = TraceExtension.RayDirection( cursorOrigin, cursorAim ).Run();

				if ( trace.Entity is ISelectable target && target.Position.Distance( ability.User.Position ) < ability.MaxDistance )
				{
					TargetCircle.Position = target.Position;
					TargetCircle.TargetSize = target.GetDiameterXY( 1.3f, true );

					if ( ability.IsTargetValid( target ) )
					{
						if ( Input.Down( InputButton.PrimaryAttack ) )
						{
							StopSelectingTarget();
							UseOnTarget( ability.User.NetworkIdent, ability.UniqueId, target.NetworkIdent );
							return;
						}

						TargetCircle.TargetColor = Color.Green;
					}
					else
					{
						TargetCircle.TargetColor = Color.Red;
					}
				}
				else
				{
					TargetCircle.Position = trace.EndPosition;
					TargetCircle.TargetColor = Color.Red;
					TargetCircle.TargetSize = MathF.Max( ability.AreaOfEffectRadius * 0.8f, 100f );
				}
			}

			if ( Input.Down( InputButton.SecondaryAttack ) )
			{
				StopSelectingTarget();
			}
		}
	}
}
