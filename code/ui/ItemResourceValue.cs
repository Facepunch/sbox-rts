
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class ItemResourceValue : Panel
	{
		public Panel Icon { get; private set; }
		public Label Label { get; set; }
		public int NumericValue
		{
			get
			{
				return Convert.ToInt32( Label.Text );
			}

			set
			{
				LerpToStartValue = value;
				LerpToEndValue = value;
				Label.Text = value.ToString();
			}
		}

		private int LerpToEndValue;
		private int LerpToStartValue;
		private float LerpToDuration;
		private RealTimeUntil LerpToEndTime;

		public ItemResourceValue()
		{
			StyleSheet.Load( "/ui/ItemResourceValue.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void LerpTo( int value, float duration )
		{
			LerpToStartValue = NumericValue;
			LerpToEndValue = value;
			LerpToEndTime = duration;
			LerpToDuration = duration;
		}

		public void Update( ResourceType type, int value )
		{
			var icon = type.GetIcon();

			if ( icon != null )
			{
				Icon.Style.BackgroundImage = icon;
				Icon.Style.BackgroundSizeX = Length.Percent( 100f );
				Icon.Style.BackgroundSizeY = Length.Percent( 100f );
			}
			else
			{
				Icon.Style.BackgroundImage = null;
				Icon.Style.BackgroundSizeX = null;
				Icon.Style.BackgroundSizeY = null;
			}

			NumericValue = value;
		}

		public void SetAffordable( bool isAffordable )
		{
			SetClass( "affordable", isAffordable );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}

		public override void Tick()
		{
			UpdateLerp();

			base.Tick();
		}

		private void UpdateLerp()
		{
			if ( LerpToStartValue == LerpToEndValue )
			{
				SetClass( "lerping", false );
				return;
			}

			var fraction = Easing.EaseInOut( ((LerpToDuration - LerpToEndTime) / LerpToDuration).Clamp( 0f, 1f ) );

			if ( fraction == 1f )
			{
				NumericValue = LerpToEndValue;
				return;
			}

			Label.Text = MathF.Ceiling( LerpToStartValue + ((LerpToEndValue - LerpToStartValue) * fraction) ).ToString();

			SetClass( "lerping", true );
		}
	}
}
