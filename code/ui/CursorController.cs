
using Facepunch.RTS;
using Gamelib.DayNight;
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class CursorController : Panel
	{
		public Vector2 StartSelection { get; private set; }
		public SpotLightEntity SpotLight { get; private set; }
		public Panel SelectionArea { get; private set; }
		public Rect SelectionRect { get; private set; }
		public bool IsSelecting { get; private set; }
		public bool IsMultiSelect { get; private set; }

		private RealTimeSince _lastSlotPressTime;
		private float _spotLightBrightness;
		private bool _lookAtSelection;
		private int _slotPressed;

		public CursorController()
		{
			StyleSheet.Load( "/ui/CursorController.scss" );

			SelectionArea = Add.Panel( "selection" );

			SpotLight = new SpotLightEntity
			{
				Color = Color.White,
				Range = 1500f,
				Enabled = false,
				OuterConeAngle = 50f,
				InnerConeAngle = 30f,
				BrightnessMultiplier = 5f,
				Rotation = Rotation.LookAt( Vector3.Down ),
				Falloff = 1000f
			};

			DayNightManager.OnSectionChanged += HandleTimeSectionChanged;
		}

		private void HandleTimeSectionChanged( TimeSection section )
		{
			if ( section == TimeSection.Dusk || section == TimeSection.Night )
				_spotLightBrightness = 5f;
			else
				_spotLightBrightness = 0f;
		}

		public override void Tick()
		{
			SelectionArea.SetClass( "hidden", !IsSelecting || !IsMultiSelect );

			if ( SpotLight.Enabled )
			{
				var trace = TraceExtension.RayDirection( Input.Cursor.Origin, Input.Cursor.Direction )
					.WorldOnly()
					.Run();

				SpotLight.Position = trace.EndPos + Vector3.Up * 1000f;
			}

			SpotLight.Brightness = SpotLight.Brightness.LerpTo( _spotLightBrightness, Time.Delta * 5f );
			SpotLight.Enabled = (SpotLight.Brightness > 0f);

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

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			if ( Items.IsGhostValid() || Abilities.IsSelectingTarget() )
				return;

			if ( Local.Pawn is not Player player )
				return;

			if ( builder.Pressed( InputButton.Attack1 ) )
			{
				StartSelection = Mouse.Position;
				IsMultiSelect = false;
				IsSelecting = true;
			}

			for ( var i = 1; i <= 9; i++ )
			{
				if ( builder.Pressed( SlotByIndex( i ) ) )
				{
					if ( builder.Down( InputButton.Run ) )
					{
						SelectionGroups.Update( i, player.GetSelected<ISelectable>() );
						break;
					}

					if ( _slotPressed == i && _lastSlotPressTime < 0.2 )
					{
						_lookAtSelection = true;
					}
					else
					{
						_lastSlotPressTime = 0;
						_lookAtSelection = false;
						_slotPressed = i;
					}

					break;
				}
			}

			if ( _slotPressed > 0 && _lastSlotPressTime > 0.2)
			{
				var selectables = SelectionGroups.GetInSlot( _slotPressed );

				if ( selectables.Count > 0 )
				{
					var list = string.Join( ",", selectables.Select( u => u.NetworkIdent ) );

					Items.Select( list, false );

					if ( _lookAtSelection )
					{
						Items.FocusCameraOn( selectables );
					}
				}

				_slotPressed = -1;
			}

			if ( builder.Released( InputButton.Jump ) )
			{
				var selectables = player.GetAllSelected();

				if ( selectables.Count > 0 )
				{
					Items.FocusCameraOn( selectables );
				}
			}

			if ( builder.Released( InputButton.Attack2 ) )
			{
				var isHoldingShift = Input.Down( InputButton.Run );

				if ( player.Selection.Count > 0 )
				{
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction )
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

					trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction )
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

						if ( builder.Down( InputButton.Run ) )
						{
							if ( selectable is IOccupiableEntity occupiable && occupiable.CanOccupyUnits )
							{
								Items.Occupy( targetNetworkId );
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
						Items.MoveToLocation( trace.EndPos.ToCSV(), isHoldingShift );
					}
				}
			}

			if ( builder.Down( InputButton.Attack1 ) && IsSelecting )
			{
				var position = Mouse.Position;
				var selection = new Rect(
					Math.Min( StartSelection.x, position.x ),
					Math.Min( StartSelection.y, position.y ),
					Math.Abs( StartSelection.x - position.x ),
					Math.Abs( StartSelection.y - position.y )
				);
				SelectionArea.Style.Left = Length.Pixels( selection.left * ScaleFromScreen );
				SelectionArea.Style.Top = Length.Pixels( selection.top * ScaleFromScreen );
				SelectionArea.Style.Width = Length.Pixels( selection.width * ScaleFromScreen );
				SelectionArea.Style.Height = Length.Pixels( selection.height * ScaleFromScreen );
				SelectionArea.Style.Dirty();

				IsMultiSelect = (selection.width > 1f || selection.height > 1f);
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
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction ).EntitiesOnly().Run();

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
				var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction ).EntitiesOnly().Run();

				if ( trace.Entity is ResourceEntity resource && resource.HasBeenSeen )
				{
					ItemTooltip.Instance.Update( resource );
					ItemTooltip.Instance.Hover( trace.Entity );
					ItemTooltip.Instance.Show( 0.5f );
				}
				else if ( trace.Entity is ObstacleEntity obstacle && obstacle.HasBeenSeen )
				{
					ItemTooltip.Instance.Update( obstacle );
					ItemTooltip.Instance.Hover( trace.Entity );
					ItemTooltip.Instance.Show( 0.5f );
				}
			}
		}
	}
}
