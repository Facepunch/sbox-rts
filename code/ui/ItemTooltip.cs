
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
			Cost = Add.Label( "cost" );
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
		public float HideTime { get; set; }
		public bool Show { get; set; }
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

		public void Hover( Panel target )
		{
			var targetBox = target.Box.Rect * ScaleFromScreen;

			Style.Left = Length.Pixels( targetBox.Center.x );
			Style.Top = Length.Pixels( targetBox.top - 32 );
			Style.Dirty();
		}

		public void Update( BaseItem item )
		{
			Name.Text = item.Name;
			Desc.Text = item.Description;

			foreach ( var kv in Costs )
			{
				if ( item.Costs.TryGetValue( kv.Key, out var cost ) )
				{
					kv.Value.Cost.Text = cost.ToString();
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
			SetClass( "hidden", !Show || (HideTime > 0f && Time.Now >= HideTime) );

			if ( Show && HideTime > 0f && Time.Now > HideTime )
			{
				HideTime = 0f;
				Show = false;
			}

			base.Tick();
		}

		private void AddResource( ResourceType type )
		{
			var cost = AddChild<ItemResourceCost>();
			cost.Update( type, 0 );
			Costs.Add( type, cost );
		}
	}
}
