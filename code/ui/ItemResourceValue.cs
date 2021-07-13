
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
				_lerpToStartValue = value;
				_lerpToEndValue = value;
				Label.Text = value.ToString();
			}
		}

		private int _lerpToEndValue;
		private int _lerpToStartValue;
		private float _lerpToDuration;
		private RealTimeUntil _lerpToEndTime;

		public ItemResourceValue()
		{
			StyleSheet.Load( "/ui/ItemResourceValue.scss" );

			Icon = Add.Panel( "icon" );
			Label = Add.Label( "", "label" );
		}

		public void LerpTo( int value, float duration )
		{
			_lerpToEndValue = value;
			_lerpToEndTime = duration;
			_lerpToDuration = duration;
		}

		public void Update( ResourceType type, int value )
		{
			var icon = type.GetIcon();

			if ( icon != null )
			{
				Icon.Style.Background = new PanelBackground
				{
					SizeX = Length.Percent( 100f ),
					SizeY = Length.Percent( 100f ),
					Texture = icon
				};

				Icon.Style.Dirty();
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
			if ( _lerpToStartValue == _lerpToEndValue )
			{
				SetClass( "lerping", false );
				return;
			}

			var fraction = Easing.EaseInOut( ((_lerpToDuration - _lerpToEndTime) / _lerpToDuration).Clamp( 0f, 1f ) );

			if ( fraction == 1f )
			{
				NumericValue = _lerpToEndValue;
				return;
			}

			Label.Text = MathF.Ceiling( _lerpToStartValue + ((_lerpToEndValue - _lerpToStartValue) * fraction) ).ToString();

			SetClass( "lerping", true );
		}
	}
}
