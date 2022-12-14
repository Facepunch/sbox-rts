using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/ItemLabelValue.scss" )]
	public class ItemLabelValue : Panel
	{
		public Label Label { get; set; }

		public ItemLabelValue()
		{
			Label = Add.Label( "", "label" );
		}

		public void Update( ItemLabel data )
		{
			Style.BackgroundColor = (data.Color * 0.7f).WithAlpha( 0.2f );
			Style.Dirty();

			Label.Style.FontColor = data.Color;
			Label.Style.Dirty();

			Label.Text = data.Text;
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
