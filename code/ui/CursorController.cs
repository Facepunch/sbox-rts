
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
		public Panel SelectionArea { get; private set; }
		public Rect SelectionRect { get; private set; }
		public bool IsSelecting { get; private set; }
		public bool IsMultiSelect { get; private set; }

		public CursorController()
		{
			StyleSheet.Load( "/ui/CursorController.scss" );

			SelectionArea = Add.Panel( "selection" );
		}

		public override void Tick()
		{
			SelectionArea.SetClass( "hidden", !IsSelecting || !IsMultiSelect );

			base.Tick();
		}

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			if ( builder.Pressed( InputButton.Attack1 ) )
			{
				StartSelection = Mouse.Position;
				IsMultiSelect = false;
				IsSelecting = true;
			}

			if ( builder.Released( InputButton.Attack2 ) && !ItemManager.Ghost.IsValid() )
			{
				if ( Local.Pawn is Player player && player.Selection.Count > 0 )
				{
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction )
						.Radius( 5f )
						.Run();

					if ( trace.Entity is ISelectable selectable )
					{
						if ( selectable.Player != Local.Pawn )
						{
							ItemManager.Attack( ((Entity)selectable).NetworkIdent );
						}
						else if ( selectable is BuildingEntity building  )
						{
							var selectedNetworkId = ((Entity)selectable).NetworkIdent;

							if ( building.IsUnderConstruction )
								ItemManager.Construct( selectedNetworkId );
							else if ( building.CanDepositResources )
								ItemManager.Deposit( selectedNetworkId );
							else if ( building.CanOccupyUnits )
								ItemManager.Occupy( selectedNetworkId );
						}
					}
					else if ( trace.Entity is ResourceEntity resource)
					{
						ItemManager.Gather( resource.NetworkIdent );
					}
					else
					{
						ItemManager.MoveToLocation( trace.EndPos.ToCSV() );
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

					ItemManager.Select( list );
				}
				else
				{
					var trace = TraceExtension.RayDirection( builder.Cursor.Origin, builder.Cursor.Direction ).EntitiesOnly().Run();

					if ( trace.Entity is ISelectable selectable && selectable.CanSelect() )
						ItemManager.Select( trace.Entity.NetworkIdent.ToString() );
					else
						ItemManager.Select();
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
