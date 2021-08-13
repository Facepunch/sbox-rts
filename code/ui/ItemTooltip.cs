
using Gamelib.Extensions;
using Facepunch.RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class ItemTooltip : Panel
	{
		public static ItemTooltip Instance { get; private set; }

		public Label Name { get; private set; }
		public Label Desc { get; private set; }
		public float HideTime { get; private set; }
		public bool IsShowing { get; private set; }
		public object Target { get; private set; }
		public PopulationLabel Population { get; private set; }
		public ItemResourceValues Costs { get; private set; }
		public DependencyList Dependencies { get; private set; }
		public ResistanceValues Resistances { get; private set; }

		public ItemTooltip()
		{
			StyleSheet.Load( "/ui/ItemTooltip.scss" );

			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );

			Instance = this;

			Costs = AddChild<ItemResourceValues>( "costs" );
			Dependencies = AddChild<DependencyList>( "dependencies" );
			Resistances = AddChild<ResistanceValues>( "resistances" );

			Costs.AddResource( ResourceType.Stone );
			Costs.AddResource( ResourceType.Beer );
			Costs.AddResource( ResourceType.Metal );
			Costs.AddResource( ResourceType.Plasma );

			foreach ( var kv in RTS.Resistances.Table )
			{
				Resistances.AddResistance( kv.Value );
			}

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

		public void Update( BaseAbility ability, bool showDependencies = false )
		{
			var player = Local.Pawn as Player;

			Name.Style.FontColor = ability.Color;
			Name.Style.Dirty();

			Name.Text = ability.Name;
			Desc.Text = ability.GetDescription();

			Dependencies.SetVisible( showDependencies );

			if ( !showDependencies )
			{
				Costs.SetVisible( true );

				foreach ( var kv in Costs.Values )
				{
					if ( ability.Costs.TryGetValue( kv.Key, out var cost ) )
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
			}
			else
			{
				Dependencies.Clear();

				foreach ( var dependency in ability.Dependencies )
				{
					var requiredItem = Items.Find<BaseItem>( dependency );

					if ( !requiredItem.Has( player ) )
					{
						Dependencies.AddDependency( requiredItem );
					}
				}

				Costs.SetVisible( false );
			}

			Resistances.SetVisible( false );

			Population.SetClass( "hidden", true );
		}

		public void Update( ResourceEntity entity )
		{
			Name.Style.FontColor = entity.Resource.GetColor();
			Name.Style.Dirty();

			Name.Text = entity.Name;
			Desc.Text = entity.Description;

			foreach ( var kv in Costs.Values )
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

			Dependencies.SetVisible( false );
			Resistances.SetVisible( false );
			Population.SetVisible( false );
		}

		public void Update( BaseItem item, bool hideCosts = false, bool showDependencies = false )
		{
			var player = Local.Pawn as Player;

			Name.Style.FontColor = item.Color;
			Name.Style.Dirty();

			Name.Text = item.Name;
			Desc.Text = item.Description;

			Dependencies.SetVisible( showDependencies );
			Resistances.SetVisible( false );

			if ( !showDependencies )
			{
				Costs.SetVisible( !hideCosts );

				if ( !hideCosts )
				{
					foreach ( var kv in Costs.Values )
					{
						if ( item.Costs.TryGetValue( kv.Key, out var cost ) )
						{
							kv.Value.Label.Text = cost.ToString();
							kv.Value.SetAffordable( player.GetResource( kv.Key ) >= cost );
							kv.Value.SetVisible( true );
						}
						else
						{
							kv.Value.SetVisible( false );
						}
					}
				}

				if ( item is BaseUnit unit )
				{
					Population.Label.SetClass( "full", !player.HasPopulationCapacity( unit.Population ) );
					Population.Label.Text = unit.Population.ToString();
					Population.SetVisible( true );
				}
				else
				{
					Population.SetVisible( false );
				}
			}
			else
			{
				Dependencies.Clear();

				foreach ( var dependency in item.Dependencies )
				{
					var requiredItem = Items.Find<BaseItem>( dependency );

					if ( !requiredItem.Has( player ) ) 
					{
						Dependencies.AddDependency( requiredItem );
					}
				}

				Population.SetVisible( false );
				Costs.SetVisible( false );
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
	}
}
