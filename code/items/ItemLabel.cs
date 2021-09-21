using Sandbox;

namespace Facepunch.RTS
{
    public struct ItemLabel
	{
		public string Text { get; set; }
		public Color Color { get; set; }

		public ItemLabel( string text, Color color )
		{
			Text = text;
			Color = color;
		}

		public ItemLabel( string text )
		{
			Text = text;
			Color = Color.Magenta;
		}
	}
}
