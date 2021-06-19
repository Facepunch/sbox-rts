
using Gamelib.Extensions;
using RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
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
			Icon.AddClass( type.ToString().ToLower() );
			NumericValue = value;
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

	public class ItemTooltip : Panel
	{
		public static ItemTooltip Instance { get; private set; }

		public Label Name { get; private set; }
		public Label Desc { get; private set; }
		public float HideTime { get; private set; }
		public bool IsShowing { get; private set; }
		public object Target { get; private set; }
		public PopulationLabel Population { get; private set; }
		public Panel CostContainer { get; private set; }
		public Dictionary<ResourceType, ItemResourceValue> Costs { get; private set; }

		public ItemTooltip()
		{
			StyleSheet.Load( "/ui/ItemTooltip.scss" );

			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );

			Costs = new();
			Instance = this;

			CostContainer = Add.Panel( "costs" );

			AddResource( ResourceType.Stone );
			AddResource( ResourceType.Beer );
			AddResource( ResourceType.Metal );
			AddResource( ResourceType.Plasma );

			Population = AddChild<PopulationLabel>( "population" );
		}

		public void Show( float hideTime = 0f )
		{
			IsShowing = true;
			HideTime = hideTime > 0f ? Time.Now + hideTime : 0f;
		}

		public void Hide()
		{
			IsShowing = false;
			HideTime = 0f;
		}

		public void Hover( Entity entity )
		{
			Target = entity;
			UpdatePosition();
		}

		public void Hover( Panel panel )
		{
			Target = panel;
			UpdatePosition();
		}

		public void Update( ResourceEntity entity )
		{
			Name.Style.FontColor = entity.Color;
			Name.Style.Dirty();

			Name.Text = entity.Name;
			Desc.Text = entity.Description;

			foreach ( var kv in Costs )
			{
				if ( kv.Key == entity.Resource )
				{
					kv.Value.Label.Text = entity.Stock.ToString();
					kv.Value.SetClass( "affordable", true );
					kv.Value.SetClass( "hidden", false );
				}
				else
				{
					kv.Value.SetClass( "hidden", true );
				}
			}

			Population.SetClass( "hidden", true );
		}

		public void Update( BaseItem item )
		{
			var player = Local.Pawn as Player;

			Name.Style.FontColor = item.Color;
			Name.Style.Dirty();

			Name.Text = item.Name;
			Desc.Text = item.Description;

			foreach ( var kv in Costs )
			{
				if ( item.Costs.TryGetValue( kv.Key, out var cost ) )
				{
					kv.Value.Label.Text = cost.ToString();
					kv.Value.SetClass( "affordable", player.GetResource( kv.Key ) >= cost );
					kv.Value.SetClass( "hidden", false );
				}
				else
				{
					kv.Value.SetClass( "hidden", true );
				}
			}

			if ( item is BaseUnit unit )
			{
				Population.Label.SetClass( "full", !player.HasPopulationCapacity( unit.Population ) );
				Population.Label.Text = unit.Population.ToString();
				Population.SetClass( "hidden", false );
			}
			else
			{
				Population.SetClass( "hidden", true );
			}
		}

		public override void Tick()
		{
			SetClass( "hidden", !IsShowing || (HideTime > 0f && Time.Now >= HideTime) );

			if ( IsShowing && HideTime > 0f && Time.Now > HideTime )
			{
				HideTime = 0f;
				IsShowing = false;
			}

			if ( IsShowing )
				UpdatePosition();

			base.Tick();
		}

		private void UpdatePosition()
		{
			if ( Target is Panel panel )
			{
				var targetBox = panel.Box.Rect * ScaleFromScreen;

				Style.Left = Length.Pixels( targetBox.Center.x );
				Style.Top = Length.Pixels( targetBox.top - 32 );
				Style.Dirty();
			}
			else if ( Target is Entity entity && entity.IsValid() )
			{
				var position = entity.Position.ToScreen() * new Vector3( Screen.Width, Screen.Height ) * ScaleFromScreen;

				Style.Left = Length.Pixels( position.x );
				Style.Top = Length.Pixels( position.y - 32 );
				Style.Dirty();
			}
		}

		private void AddResource( ResourceType type )
		{
			var cost = CostContainer.AddChild<ItemResourceValue>();
			cost.Update( type, 0 );
			Costs.Add( type, cost );
		}
	}
}
