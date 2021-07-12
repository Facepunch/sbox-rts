
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

		private float _spotLightBrightness;

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
				var trace = TraceExtension.RayDirection( Input.Cursor.Origin, Input.Cursor.Direction ).EntitiesOnly().Run();
				SpotLight.Position = trace.EndPos + Vector3.Up * 750f;
			}

			SpotLight.Brightness = SpotLight.Brightness.LerpTo( _spotLightBrightness, Time.Delta * 5f );
			SpotLight.Enabled = (SpotLight.Brightness > 0f);

			base.Tick();
		}

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			if ( Items.IsPlacingBuilding() || Abilities.IsSelectingTarget() )
				return;

			if ( builder.Pressed( InputButton.Attack1 ) )
			{
				StartSelection = Mouse.Position;
				IsMultiSelect = false;
				IsSelecting = true;
			}

			if ( builder.Released( InputButton.Attack2 ) )
			{
				if ( Local.Pawn is Player player && player.Selection.Count > 0 )
				{
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction )
						.Radius( 5f )
						.Run();

					if ( trace.Entity is ISelectable selectable )
					{
						var targetNetworkId = ((Entity)selectable).NetworkIdent;

						if ( selectable is IOccupiableEntity occupiable )
						{
							if ( occupiable.CanOccupyUnits )
							{
								Items.Occupy( targetNetworkId );
								return;
							}
						}

						if ( !selectable.IsLocalPlayers )
						{
							Items.Attack( ((Entity)selectable).NetworkIdent );
						}
						else if ( selectable is BuildingEntity building  )
						{
							if ( building.IsUnderConstruction )
								Items.Construct( targetNetworkId );
							else if ( building.CanDepositResources )
								Items.Deposit( targetNetworkId );
						}
					}
					else if ( trace.Entity is ResourceEntity resource)
					{
						Items.Gather( resource.NetworkIdent );
					}
					else
					{
						Items.MoveToLocation( trace.EndPos.ToCSV() );
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
								entities.Add( entity.NetworkIdent );
							}
						}
					}

					var list = string.Join( ",", entities );

					Items.Select( list );
				}
				else
				{
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction ).EntitiesOnly().Run();

					if ( trace.Entity is ISelectable selectable && selectable.CanSelect() )
						Items.Select( trace.Entity.NetworkIdent.ToString() );
					else
						Items.Select();
				}

				IsSelecting = false;
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
			}
		}
	}
}
