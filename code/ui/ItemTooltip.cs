
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
	public class ItemResourceCost : Panel
	{
		public Panel Icon { get; private set; }
		public Label Cost { get; set; }

		public ItemResourceCost()
		{
			Icon = Add.Panel( "icon" );
			Cost = Add.Label( "", "cost" );
		}

		public void Update( ResourceType type, int cost )
		{
			Icon.AddClass( type.ToString().ToLower() );
			Cost.Text = cost.ToString();
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
		public Dictionary<ResourceType, ItemResourceCost> Costs { get; private set; }

		public ItemTooltip()
		{
			StyleSheet.Load( "/ui/ItemTooltip.scss" );

			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );

			Costs = new();
			Instance = this;

			AddResource( ResourceType.Stone );
			AddResource( ResourceType.Beer );
			AddResource( ResourceType.Metal );
			AddResource( ResourceType.Plasma );
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
			Name.Text = entity.Name;
			Desc.Text = entity.Description;

			foreach ( var kv in Costs )
			{
				if ( kv.Key == entity.Resource )
				{
					kv.Value.Cost.Text = entity.Stock.ToString();
					kv.Value.SetClass( "affordable", true );
					kv.Value.SetClass( "hidden", false );
				}
				else
				{
					kv.Value.SetClass( "hidden", true );
				}
			}
		}

		public void Update( BaseItem item )
		{
			var player = Local.Pawn as Player;

			Name.Text = item.Name;
			Desc.Text = item.Description;

			foreach ( var kv in Costs )
			{
				if ( item.Costs.TryGetValue( kv.Key, out var cost ) )
				{
					kv.Value.Cost.Text = cost.ToString();
					kv.Value.SetClass( "affordable", player.GetResource( kv.Key ) >= cost );
					kv.Value.SetClass( "hidden", false );
				}
				else
				{
					kv.Value.SetClass( "hidden", true );
				}
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
			var cost = AddChild<ItemResourceCost>();
			cost.Update( type, 0 );
			Costs.Add( type, cost );
		}
	}
}
