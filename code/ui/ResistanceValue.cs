using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/ResistanceValue.scss" )]
	public class ResistanceValue : Panel
	{
		public Panel Icon { get; private set; }
		public Label Label { get; set; }

		public ResistanceValue()
		{
			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void Update( BaseResistance resistance, float value )
		{
			if ( resistance.Icon != null )
			{
				Icon.Style.BackgroundImage = resistance.Icon;
				Icon.Style.BackgroundSizeX = Length.Percent( 100f );
				Icon.Style.BackgroundSizeY = Length.Percent( 100f );
			}
			else
			{
				Icon.Style.BackgroundImage = null;
				Icon.Style.BackgroundSizeX = null;
				Icon.Style.BackgroundSizeY = null;
			}


			Update( value );
		}

		public void Update( float value )
		{
			var percentage = (value * 100f).FloorToInt();
			SetClass( "weakness", percentage < 0 );
			Label.Text = $"{percentage}%";
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
