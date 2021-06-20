
using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class ItemCommand : Button
	{
		public ISelectable Selectable { get; private set; } 
		public BaseItem Item { get; private set; }

		public ItemCommand() : base()
		{

		}

		protected override void OnClick( MousePanelEvent e )
		{
			var player = (Local.Pawn as Player);
			var status = Item.CanCreate( player );

			if ( status != ItemCreateError.Success )
			{
				RTS.Sound.Play( status );
				return;
			}

			if ( Selectable is UnitEntity worker && Item is BaseBuilding building )
				RTS.Item.CreateGhost( worker, building );
			else
				ItemManager.Queue( Selectable.NetworkIdent, Item.NetworkId );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = RTS.Tooltip;
			tooltip.Update( Item );
			tooltip.Hover( this );
			tooltip.Show();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			RTS.Tooltip.Hide();
		}

		public void Update( ISelectable selectable, BaseItem item )
		{
			Selectable = selectable;
			Item = item;

			if ( item.Icon != null )
			{
				Style.Background = new PanelBackground
				{
					SizeX = Length.Percent( 100f ),
					SizeY = Length.Percent( 100f ),
					Texture = item.Icon
				};

				Style.Dirty();
			}
		}
	}

	public class ItemOccupantButton : Button
	{
		public BuildingEntity Building { get; private set; }
		public UnitEntity Unit { get; private set; }

		protected override void OnClick( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;

			ItemManager.Evict( Building.NetworkIdent, Unit.NetworkIdent );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;

			var tooltip = RTS.Tooltip;
			tooltip.Update( Unit.Item, true );
			tooltip.Hover( this );
			tooltip.Show();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;
			
			RTS.Tooltip.Hide();
		}

		public void Update( BuildingEntity building = null, UnitEntity unit = null )
		{
			Style.Background = null;

			Building = building;
			Unit = unit;

			if ( unit.IsValid() )
			{
				var item = unit.Item;

				if ( item.Icon != null )
				{
					Style.Background = new PanelBackground
					{
						SizeX = Length.Percent( 100f ),
						SizeY = Length.Percent( 100f ),
						Texture = item.Icon
					};
				}
			}

			Style.Dirty();
		}

		public override void Tick()
		{
			SetClass( "hidden", !Building.IsValid() );
		}
	}

	public class ItemOccupantList : Panel
	{
		public BuildingEntity Building { get; private set; }
		public List<ItemOccupantButton> Buttons { get; private set; }

		public ItemOccupantList()
		{
			Buttons = new();

			for ( var i = 0; i < 10; i++ )
			{
				Buttons.Add( AddChild<ItemOccupantButton>() );
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
				if ( Building.Item.MaxOccupants > 0 )
				{
					var occupants = Building.Occupants;
					var occupantsCount = occupants.Count;
					var buildingItem = Building.Item;

					for ( var i = 0; i < 10; i++ )
					{
						if ( buildingItem.MaxOccupants > i )
						{
							if ( occupantsCount > i )
								Buttons[i].Update( Building, occupants[i] );
							else
								Buttons[i].Update( Building );
						}
						else
						{
							Buttons[i].Update( null );
						}
					}
				}
				else
				{
					SetClass( "hidden", true );
				}
			}

			base.Tick();
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

		protected override void OnClick( MousePanelEvent e )
		{
			ItemManager.Unqueue( Building.NetworkIdent, QueueItem.Id );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = RTS.Tooltip;
			tooltip.Update( QueueItem.Item );
			tooltip.Hover( this );
			tooltip.Show();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			RTS.Tooltip.Hide();
		}

		public void Update( QueueItem queueItem, BuildingEntity building = null )
		{
			if ( QueueItem != null )
				RemoveClass( QueueItem.Item.UniqueId.Replace( '.', '_' ) );

			QueueItem = queueItem;
			Building = building;

			if ( QueueItem != null )
			{
				var item = QueueItem.Item;

				AddClass( item.UniqueId.Replace( '.', '_' ) );

				if ( item.Icon != null )
				{
					Style.Background = new PanelBackground
					{
						SizeX = Length.Percent( 100f ),
						SizeY = Length.Percent( 100f ),
						Texture = item.Icon
					};

					Style.Dirty();
				}	 
			}
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

			Buttons.ForEach( b => b.Delete( true ) );
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
				var dependency = RTS.Item.Find<BaseItem>( v );

				if ( dependency.CanHave( player ) )
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
		public ItemOccupantList OccupantList { get; private set; }


		public ItemInformation()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );
			Health = Add.Label( "", "health" );
			Kills = Add.Label( "", "kills" );
			QueueList = AddChild<ItemQueueList>();
			OccupantList = AddChild<ItemOccupantList>();
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

			Name.Style.FontColor = data.Color;
			Name.Style.Dirty();

			QueueList.Update( entity );
			OccupantList.Update( entity );
		}

		private void UpdateUnit( UnitEntity entity ) 
		{
			var data = entity.Item;

			Name.Text = data.Name;
			Desc.Text = data.Description;

			Name.Style.FontColor = data.Color;
			Name.Style.Dirty();

			QueueList.Update( null );
			OccupantList.Update( null );
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

		private int _oldSelectedHashCode;

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
			if ( RTS.Game == null ) return;

			var isHidden = true;
			var round = RTS.Game.Round;

			if ( round is PlayRound )
			{
				if ( Local.Pawn is Player player )
				{
					var collection = player.Selection.Cast<ISelectable>().ToList();
					var hashCode = collection.GetItemsHashCode();

					if ( _oldSelectedHashCode != hashCode )
					{
						Update( collection );
						_oldSelectedHashCode = hashCode;
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
