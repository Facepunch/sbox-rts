using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Facepunch.RTS
{
	public class ResistanceValue : Panel
	{
		public Panel Icon { get; private set; }
		public Label Label { get; set; }


		public ResistanceValue()
		{
			StyleSheet.Load( "/ui/ResistanceValue.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void Update( BaseResistance resistance, float value )
		{
			if ( resistance.Icon != null )
			{
				Icon.Style.Background = new PanelBackground
				{
					SizeX = Length.Percent( 100f ),
					SizeY = Length.Percent( 100f ),
					Texture = resistance.Icon
				};

				Icon.Style.Dirty();
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
