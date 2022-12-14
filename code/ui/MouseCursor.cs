
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/MouseCursor.scss" )]
	public class MouseCursor : Panel
	{
		public static MouseCursor Instance { get; private set; }

		public Image Cursor { get; private set; }

		public MouseCursor() : base()
		{
			Cursor = Add.Image( "", "cursor" );
			Cursor.SetTexture( "ui/cursor/normal.png" );

			Instance = this;
		}

		public override void Tick()
		{
			var mousePosition = Mouse.Position / Screen.Size;

			Cursor.Style.Left = Length.Fraction( mousePosition.x );
			Cursor.Style.Top = Length.Fraction( mousePosition.y );
			Cursor.Style.Dirty();

			base.Tick();
		}
	}
}
