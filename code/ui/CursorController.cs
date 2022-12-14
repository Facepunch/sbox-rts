
using Facepunch.RTS;
using Gamelib.DayNight;
using Gamelib.Extensions;
using Gamelib.UI;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/CursorController.scss" )]
	public class CursorController : Panel
	{
		public Vector2 StartSelection { get; private set; }
		public Panel SelectionArea { get; private set; }
		public Rect SelectionRect { get; private set; }
		public bool IsSelecting { get; private set; }
		public bool IsMultiSelect { get; private set; }

		private RealTimeSince LastSlotPressTime;
		private bool LookAtSelection;
		private int SlotPressed;

		public CursorController()
		{
			SelectionArea = Add.Panel( "selection" );
		}

		public override void Tick()
		{
			SelectionArea.SetClass( "hidden", !IsSelecting || !IsMultiSelect );

			base.Tick();
		}

		private InputButton SlotByIndex( int index )
		{
			switch ( index )
			{
				case 0:
					return InputButton.Slot0;
				case 1:
					return InputButton.Slot1;
				case 2:
					return InputButton.Slot2;
				case 3:
					return InputButton.Slot3;
				case 4:
					return InputButton.Slot4;
				case 5:
					return InputButton.Slot5;
				case 6:
					return InputButton.Slot6;
				case 7:
					return InputButton.Slot7;
				case 8:
					return InputButton.Slot8;
				case 9:
					return InputButton.Slot0;
			}

			return InputButton.Slot0;
		}

		[Event.Client.BuildInput]
		private void BuildInput()
		{
			if ( Items.IsGhostValid() || Abilities.IsSelectingTarget() )
				return;

			if ( Game.LocalPawn is not RTSPlayer player )
				return;

			if ( !Hud.IsLocalPlaying() )
				return;

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				StartSelection = Mouse.Position;
				IsMultiSelect = false;
				IsSelecting = true;
			}

			for ( var i = 1; i <= 9; i++ )
			{
				if ( Input.Pressed( SlotByIndex( i ) ) )
				{
					if ( Input.Down( InputButton.Duck ) )
					{
						SelectionGroups.Update( i, player.GetSelected<ISelectable>() );
						break;
					}

					if ( SlotPressed == i && LastSlotPressTime < 0.2 )
					{
						LookAtSelection = true;
					}
					else
					{
						LastSlotPressTime = 0;
						LookAtSelection = false;
						SlotPressed = i;
					}

					break;
				}
			}

			if ( SlotPressed > 0 && LastSlotPressTime > 0.2)
			{
				var selectables = SelectionGroups.GetInSlot( SlotPressed );

				if ( selectables.Count > 0 )
				{
					var list = string.Join( ",", selectables.Select( u => u.NetworkIdent ) );

					Items.Select( list, false );

					if ( LookAtSelection )
					{
						Items.FocusCameraOn( selectables );
					}
				}

				SlotPressed = -1;
			}

			if ( Input.Released( InputButton.Jump ) )
			{
				var selectables = player.GetAllSelected();

				if ( selectables.Count > 0 )
				{
					Items.FocusCameraOn( selectables );
				}
			}

			if ( Input.Released( InputButton.SecondaryAttack ) )
			{
				var isHoldingShift = Input.Down( InputButton.Run );

				if ( player.Selection.Count > 0 )
				{
					var trace = TraceExtension.RayDirection( player.CursorOrigin, player.CursorDirection )
						.WithAnyTags( "blueprint" )
						.Radius( 5f )
						.Run();

					// We need to check this layer first because blueprints will be on the Debris layer.
					if ( trace.Entity is BuildingEntity blueprint && blueprint.IsLocalPlayers )
					{
						if ( blueprint.IsUnderConstruction )
						{
							Items.Construct( trace.Entity.NetworkIdent, isHoldingShift );
							return;
						}
					}

					trace = TraceExtension.RayDirection( player.CursorOrigin, player.CursorDirection )
						.WithoutTags( "blueprint" )
						.Radius( 5f )
						.Run();

					if ( trace.Entity is ISelectable selectable )
					{
						var targetNetworkId = trace.Entity.NetworkIdent;

						if ( !selectable.IsLocalPlayers )
						{
							if ( !selectable.IsLocalTeamGroup )
								Items.Attack( targetNetworkId, isHoldingShift );

							return;
						}

						if ( Input.Down( InputButton.Run ) )
						{
							if ( selectable is IOccupiableEntity occupiable && occupiable.CanOccupyUnits )
							{
								Items.Occupy( targetNetworkId, isHoldingShift );
								return;
							}
						}

						if ( selectable is BuildingEntity building  )
						{
							if ( building.IsUnderConstruction )
							{
								Items.Construct( targetNetworkId, isHoldingShift );
								return;
							}
							else
							{
								Items.RepairOrDeposit( targetNetworkId, isHoldingShift );
								return;
							}
						}
					}
					else if ( trace.Entity is IDamageable damageable )
					{
						Items.Attack( trace.Entity.NetworkIdent, isHoldingShift );
					}
					else if ( trace.Entity is ResourceEntity resource)
					{
						Items.Gather( resource.NetworkIdent, isHoldingShift );
					}
					else
					{
						Items.MoveToLocation( trace.EndPosition.ToCSV(), isHoldingShift );
					}
				}
			}

			if ( Input.Down( InputButton.PrimaryAttack ) && IsSelecting )
			{
				var position = Mouse.Position;
				var selection = new Rect(
					Math.Min( StartSelection.x, position.x ),
					Math.Min( StartSelection.y, position.y ),
					Math.Abs( StartSelection.x - position.x ),
					Math.Abs( StartSelection.y - position.y )
				);

				SelectionArea.Style.Left = Length.Pixels( selection.Left * ScaleFromScreen );
				SelectionArea.Style.Top = Length.Pixels( selection.Top * ScaleFromScreen );
				SelectionArea.Style.Width = Length.Pixels( selection.Width * ScaleFromScreen );
				SelectionArea.Style.Height = Length.Pixels( selection.Height * ScaleFromScreen );
				SelectionArea.Style.Dirty();

				IsMultiSelect = (selection.Width > 1f || selection.Height > 1f);
				SelectionRect = selection;
			}
			else if ( IsSelecting )
			{
				IsSelecting = false;

				if ( IsMultiSelect )
				{
					var selectable = Entity.All.OfType<ISelectable>();
					var entities = new List<int>();

					foreach ( var b in selectable )
					{
						if ( b is Entity entity && b.CanSelect() )
						{
							var screenScale = entity.Position.ToScreen();
							var screenX = Screen.Width * screenScale.x;
							var screenY = Screen.Height * screenScale.y;

							if ( SelectionRect.IsInside( new Rect( screenX, screenY, 1f, 1f ) ) )
							{
								if ( !b.IsLocalPlayers && !Fog.IsAreaSeen( b.Position ) )
									continue;

								entities.Add( entity.NetworkIdent );
							}
						}
					}

					var list = string.Join( ",", entities );

					Items.Select( list, Input.Down( InputButton.Run ) );
				}
				else
				{
					var trace = TraceExtension.RayDirection( player.CursorOrigin, player.CursorDirection ).EntitiesOnly().Run();

					if ( trace.Entity is ISelectable selectable )
					{
						if ( !selectable.IsLocalPlayers )
						{
							if ( Fog.IsAreaSeen( selectable.Position ) )
								Items.Select( trace.Entity.NetworkIdent.ToString() );
						}
						else
						{
							if ( selectable.CanSelect() )
								Items.Select( trace.Entity.NetworkIdent.ToString(), Input.Down( InputButton.Run ) );
							else
								Items.CancelAction( trace.Entity.NetworkIdent );
						}
					}
					else
					{
						Items.Select();
					}
				}
			}
			else
			{
				var hovered = UIUtility.GetHoveredPanel();
				if ( hovered != null ) return;

				var trace = TraceExtension.RayDirection( player.CursorOrigin, player.CursorDirection ).EntitiesOnly().Run();

				if ( trace.Entity is ITooltipEntity target )
				{
					target.ShowTooltip();
				}
			}
		}
	}
}
