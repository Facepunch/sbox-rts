
using RTS.Buildings;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace RTS
{
	public class ItemCommand : Button
	{
		public BaseItem Item { get; private set; }

		public ItemCommand() : base()
		{

		}

		public override void OnEvent( string eventName )
		{
			var tooltip = ItemTooltip.Instance;

			if ( eventName == "onclick" )
			{
				
			}
			else if ( eventName == "onmouseover" )
			{
				tooltip.Update( Item );
				tooltip.Hover( this );
				tooltip.Show = true;
			}
			else if ( eventName == "onmouseout" )
			{
				tooltip.Show = false;
			}

			base.OnEvent( eventName );
		}

		public void Update( BaseItem item )
		{
			Item = item;
		}
	}

	public class ItemCommandList : Panel
	{
		public ISelectable Item { get; private set; }
		public List<ItemCommand> Buttons { get; private set; }

		public ItemCommandList()
		{
			Buttons = new();
		}

		public void Update( ISelectable item )
		{
			Item = item;

			Buttons.ForEach( b => b.Delete() );
			Buttons.Clear();

			if ( item is UnitEntity unit )
				UpdateCommands( unit.Item.Buildables );
			else if ( item is BuildingEntity building )
				UpdateCommands( building.Item.Buildables );
		}

		private void UpdateCommands( List<string> buildables )
		{
			var player = Local.Pawn as Player;

			foreach ( var v in buildables )
			{
				var dependency = Game.Instance.FindItem<BaseItem>( v );

				if ( dependency.HasDependencies( player ) )
				{
					var button = AddChild<ItemCommand>( v.Replace( '.', '_' ) );

					button.Update( dependency );

					Buttons.Add( button );
				}
			}
		}
	}

	public class ItemInformation : Panel
	{
		public Label Name { get; private set; }
		public Label Desc { get; private set; }
		public Label Health { get; private set; }
		public Label Kills { get; private set; }
		public ISelectable Item { get; private set; }


		public ItemInformation()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );
			Health = Add.Label( "", "health" );
			Kills = Add.Label( "", "kills" );
		}

		public void Update( ISelectable item )
		{
			Item = item;

			if ( item is UnitEntity unit )
				UpdateUnit( unit );
			else if ( item is BuildingEntity building )
				UpdateBuilding( building );
		}

		public override void Tick()
		{
			if ( Item == null ) return;

			if ( Item is UnitEntity unit )
			{
				Kills.Text = "Kills: " + unit.Kills.ToString();
				Kills.SetClass( "hidden", false );
			}
			else if ( Item is BuildingEntity building )
			{
				Kills.SetClass( "hidden", true );
			}

			Health.Text = Item.Health.ToString() + "hp";

			base.Tick();
		}

		private void UpdateBuilding( BuildingEntity entity )
		{
			var data = entity.Item;

			Name.Text = data.Name;
			Desc.Text = data.Description;
		}

		private void UpdateUnit( UnitEntity entity ) 
		{
			var data = entity.Item;

			Name.Text = data.Name;
			Desc.Text = data.Description;
		}
	}

	public class ItemMultiIcon : Panel
	{
		public Panel Image { get; private set; }
		public Label Health { get; private set; }
		public ISelectable Item { get; private set; }

		public ItemMultiIcon()
		{

		}

		public void Update( ISelectable item )
		{
			Item = item;
		}
	}

	public class ItemMultiDisplay : Panel
	{
		public List<ISelectable> Items { get; private set; }

		public ItemMultiDisplay()
		{

		}

		public void Update( List<ISelectable> items )
		{
			Items = items;
		}
	}

	public class SelectedItem : Panel
	{
		public static SelectedItem Instance { get; private set; }

		public List<ISelectable> Items { get; private set; }
		public ItemMultiDisplay MultiDisplay { get; private set; }
		public ItemInformation Information { get; private set; }
		public ItemCommandList CommandList { get; private set; }
		public Panel Left { get; private set; }
		public Panel Right { get; private set; }

		public SelectedItem()
		{
			StyleSheet.Load( "/ui/SelectedItem.scss" );

			Left = Add.Panel( "left" );
			Right = Add.Panel( "right" );

			MultiDisplay = Left.AddChild<ItemMultiDisplay>();
			Information = Left.AddChild<ItemInformation>();
			CommandList = Right.AddChild<ItemCommandList>();

			Items = new();

			Instance = this;
		}

		public void Update( List<ISelectable> items )
		{
			Items = items;

			if ( items.Count == 0 ) return;

			if ( items.Count > 1 )
				MultiDisplay.Update( items );
			else
				Information.Update( items[0] );

			CommandList.Update( items[0] );
		}

		public override void Tick()
		{
			if ( Game.Instance == null ) return;

			var isHidden = true;
			var round = Game.Instance.Round;

			if ( round is PlayRound )
			{
				if ( Items.Count > 0 )
					isHidden = false;
			}

			if ( Items.Count > 0 )
			{
				MultiDisplay.SetClass( "hidden", Items.Count < 2 );
				Information.SetClass( "hidden", Items.Count > 1 );
			}

			SetClass( "hidden", isHidden );

			base.Tick();
		}
	}
}
