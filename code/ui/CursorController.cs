
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
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
			if ( builder.Pressed(InputButton.Attack1) )
			{
				StartSelection = Mouse.Position;
				IsMultiSelect = false;
				IsSelecting = true;
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
					var buildings = Entity.All.OfType<BuildingEntity>();
					var entities = new List<int>();

					foreach ( var b in buildings )
					{
						var screenScale = b.Position.ToScreen();
						var screenX = Screen.Width * screenScale.x;
						var screenY = Screen.Height * screenScale.y;

						if ( SelectionRect.IsInside( new Rect( screenX, screenY, 1f, 1f ) ) )
						{
							entities.Add( b.NetworkIdent );
						}
					}

					var list = string.Join( ",", entities );

					Game.SelectItems( list );
				}
				else
				{
					var trace = Trace.Ray( builder.Position, builder.Position + builder.CursorAim * 2000f ).EntitiesOnly().Run();

					if ( trace.Entity.IsValid() )
						Game.SelectItems( trace.Entity.NetworkIdent.ToString() );
					else
						Game.SelectItems();
				}

				IsSelecting = false;
			}
		}
	}
}
