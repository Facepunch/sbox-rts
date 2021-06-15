
using RTS.Buildings;
using RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
	public class ItemCommand : Button
	{
		public ISelectable Selectable { get; private set; } 
		public BaseItem Item { get; private set; }

		public ItemCommand() : base()
		{

		}

		public override void OnEvent( string eventName )
		{
			var tooltip = ItemTooltip.Instance;

			if ( eventName == "onclick" )
			{
				if ( Selectable is UnitEntity unit && Item is BaseBuilding building )
					ItemManager.Instance.CreateGhost( unit, building );
				else
					ItemManager.Queue( Selectable.NetworkIdent, Item.NetworkId );
			}
			else if ( eventName == "onmouseover" )
			{
				tooltip.Update( Item );
				tooltip.Hover( this );
				tooltip.Show();
			}
			else if ( eventName == "onmouseout" )
			{
				tooltip.Hide();
			}

			base.OnEvent( eventName );
		}

		public void Update( ISelectable selectable, BaseItem item )
		{
			Selectable = selectable;
			Item = item;
		}
	}

	public class ItemQueueButton : Button
	{
		public BuildingEntity Building { get; private set; }
		public QueueItem QueueItem { get; private set; }
		public Panel Countdown { get; private set; }

		public ItemQueueButton() : base()
		{
			Countdown = Add.Panel( "countdown" );
		}

		public override void OnEvent( string eventName )
		{
			var tooltip = ItemTooltip.Instance;

			if ( eventName == "onclick" )
			{
				ItemManager.Unqueue( Building.NetworkIdent, QueueItem.Id );
			}
			else if ( eventName == "onmouseover" )
			{
				tooltip.Update( QueueItem.Item );
				tooltip.Hover( this );
				tooltip.Show();
			}
			else if ( eventName == "onmouseout" )
			{
				tooltip.Hide();
			}

			base.OnEvent( eventName );
		}

		public void Update( QueueItem queueItem, BuildingEntity building = null )
		{
			if ( QueueItem != null )
				RemoveClass( QueueItem.Item.UniqueId.Replace( '.', '_' ) );

			QueueItem = queueItem;
			Building = building;

			if ( QueueItem != null )
				AddClass( QueueItem.Item.UniqueId.Replace( '.', '_' ) );
		}

		public override void Tick()
		{
			SetClass( "hidden", QueueItem == null );

			if ( QueueItem != null )
			{
				if ( QueueItem.FinishTime > 0f )
					Countdown.Style.Width = Length.Percent( 100f - ((100f / QueueItem.Item.BuildTime) * QueueItem.GetTimeLeft()) );
				else
					Countdown.Style.Width = Length.Percent( 100f );

				Countdown.SetClass( "inactive", QueueItem.FinishTime == 0f );
			}

			base.Tick();
		}
	}

	public class ItemQueueList : Panel
	{
		public BuildingEntity Building { get; private set; }
		public List<ItemQueueButton> Buttons { get; private set; }

		public ItemQueueList()
		{
			Buttons = new();

			for ( var i = 0; i < 10; i++ )
			{
				Buttons.Add( AddChild<ItemQueueButton>() );
			}
		}

		public void Update( BuildingEntity building )
		{
			Building = building;
		}

		public override void Tick()
		{
			SetClass( "hidden", Building == null );

			if ( Building.IsValid() )
			{
				for ( var i = 0; i < 10; i++ )
				{
					if ( Building.Queue.Count > i )
						Buttons[i].Update( Building.Queue[i], Building );
					else
						Buttons[i].Update(  null );
				}
			}

			base.Tick();
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

		private void UpdateCommands( HashSet<string> buildables )
		{
			var player = Local.Pawn as Player;

			foreach ( var v in buildables )
			{
				var dependency = ItemManager.Instance.Find<BaseItem>( v );

				if ( dependency.HasDependencies( player ) )
				{
					var button = AddChild<ItemCommand>( v.Replace( '.', '_' ) );

					button.Update( Item, dependency );

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
		public ItemQueueList QueueList { get; private set; }


		public ItemInformation()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );
			Health = Add.Label( "", "health" );
			Kills = Add.Label( "", "kills" );
			QueueList = AddChild<ItemQueueList>();
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

			QueueList.Update( entity );
		}

		private void UpdateUnit( UnitEntity entity ) 
		{
			var data = entity.Item;

			Name.Text = data.Name;
			Desc.Text = data.Description;

			QueueList.Update( null );
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

		private int _lastCollectionHash;

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
				if ( Local.Pawn is Player player )
				{
					// TODO: This is a bit slow but we can't know when the collection is updated.
					var collection = player.Selection.Cast<ISelectable>().ToList();
					var collectionHash = 0;

					for ( var i = 0; i < collection.Count; i++ )
					{
						collectionHash += collection[i].NetworkIdent;
					}

					if ( collectionHash != _lastCollectionHash )
					{
						Update( collection );
						_lastCollectionHash = collectionHash;
					}
				}

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
