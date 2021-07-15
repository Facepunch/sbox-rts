using Facepunch.RTS.Buildings;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class ItemCommandAbility : ItemCommand
	{
		public BaseAbility Ability { get; private set; }
		public Panel Countdown { get; private set; }
		public ItemCommandAbility() : base()
		{
			Countdown = Add.Panel( "countdown" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( Selectable.IsUsingAbility() )
			{
				// We can't use another while once is being used.
				return;
			}
			
			var status = Ability.CanUse();

			if ( status != RequirementError.Success )
			{
				Audio.Play( status );
				return;
			}

			if ( Ability.TargetType == AbilityTargetType.Self )
				Abilities.UseOnSelf( Selectable.NetworkIdent, Ability.UniqueId );
			else
				Abilities.SelectTarget( Ability );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = Hud.Tooltip;
			tooltip.Update( Ability );
			tooltip.Hover( this );
			tooltip.Show();
		}

		public void Update( ISelectable selectable, BaseAbility ability )
		{
			Selectable = selectable;
			Ability = ability;

			if ( ability.Icon != null )
			{
				Style.Background = new PanelBackground
				{
					SizeX = Length.Percent( 100f ),
					SizeY = Length.Percent( 100f ),
					Texture = ability.Icon
				};

				Style.Dirty();
			}
		}

		public override void Tick()
		{
			var cooldownTimeLeft = Ability.GetCooldownTimeLeft();

			Countdown.Style.Width = Length.Percent( (100f / Ability.Cooldown) * cooldownTimeLeft );
			Countdown.Style.Dirty();

			Countdown.SetClass( "hidden", cooldownTimeLeft == 0f );

			base.Tick();
		}
	}

	public class ItemCommandQueueable : ItemCommand
	{
		public BaseItem Item { get; private set; }

		protected override void OnClick( MousePanelEvent e )
		{
			var player = (Local.Pawn as Player);
			var status = Item.CanCreate( player );

			if ( status != RequirementError.Success )
			{
				Audio.Play( status );
				return;
			}

			if ( Selectable is UnitEntity worker && Item is BaseBuilding building )
				Items.CreateGhost( worker, building );
			else
				Items.Queue( Selectable.NetworkIdent, Item.NetworkId );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = Hud.Tooltip;
			tooltip.Update( Item );
			tooltip.Hover( this );
			tooltip.Show();
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

	public abstract class ItemCommand : Button
	{
		public ISelectable Selectable { get; protected set; }

		public ItemCommand() : base()
		{

		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			Hud.Tooltip.Hide();
		}
	}

	public class ItemOccupantHealth : Panel
	{
		public Panel Foreground { get; private set; }

		public ItemOccupantHealth()
		{
			Foreground = Add.Panel( "foreground" );
		}
	}

	public class ItemOccupantButton : Button
	{
		public ItemOccupantHealth Health { get; private set; }
		public IOccupiableEntity Occupiable { get; private set; }
		public UnitEntity Unit { get; private set; }

		public ItemOccupantButton() : base()
		{
			Health = AddChild<ItemOccupantHealth>( "health" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;

			Items.Evict( Occupiable.NetworkIdent, Unit.NetworkIdent );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;

			var tooltip = Hud.Tooltip;
			tooltip.Update( Unit.Item, true );
			tooltip.Hover( this );
			tooltip.Show();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			if ( !Unit.IsValid() ) return;
			
			Hud.Tooltip.Hide();
		}

		public void Update( IOccupiableEntity occupiable = null, UnitEntity unit = null )
		{
			Style.Background = null;

			Occupiable = occupiable;
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

				Health.SetClass( "hidden", false );
			}
			else
			{
				Health.SetClass( "hidden", true );
			}

			SetClass( "empty", !unit.IsValid() );

			Style.Dirty();
		}

		public override void Tick()
		{
			SetClass( "hidden", Occupiable == null );

			if ( Unit.IsValid() )
			{
				Health.Foreground.Style.Width = Length.Fraction( Unit.Health / Unit.MaxHealth );
				Health.Foreground.Style.Dirty();
			}
		}
	}

	public class ItemOccupantList : Panel
	{
		public IOccupiableEntity Entity { get; private set; }
		public List<ItemOccupantButton> Buttons { get; private set; }

		public ItemOccupantList()
		{
			Buttons = new();

			for ( var i = 0; i < 10; i++ )
			{
				Buttons.Add( AddChild<ItemOccupantButton>() );
			}
		}

		public void Update( IOccupiableEntity occupiable )
		{
			Entity = occupiable;
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "hidden", Entity == null );

			if ( Entity == null ) return;

			var item = Entity.OccupiableItem;
			var occupants = Entity.GetOccupantsList();

			if ( item.Occupiable.MaxOccupants > 0 && occupants != null )
			{
				var occupantsCount = occupants.Count;

				for ( var i = 0; i < 10; i++ )
				{
					if ( item.Occupiable.MaxOccupants > i )
					{
						if ( occupantsCount > i )
							Buttons[i].Update( Entity, occupants[i] );
						else
							Buttons[i].Update( Entity );
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
			Items.Unqueue( Building.NetworkIdent, QueueItem.Id );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = Hud.Tooltip;
			tooltip.Update( QueueItem.Item );
			tooltip.Hover( this );
			tooltip.Show();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			Hud.Tooltip.Hide();
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
				UpdateCommands( unit.Item.Buildables, unit.Item.Abilities );
			else if ( item is BuildingEntity building )
				UpdateCommands( building.Item.Buildables, building.Item.Abilities );
		}

		private void UpdateCommands( HashSet<string> buildables, HashSet<string> abilities = null )
		{
			var player = Local.Pawn as Player;

			foreach ( var v in buildables )
			{
				var buildable = Items.Find<BaseItem>( v );

				if ( buildable.CanHave( player ) )
				{
					var button = AddChild<ItemCommandQueueable>( "command" );

					button.Update( Item, buildable );

					Buttons.Add( button );
				}
			}

			if ( abilities == null ) return;

			foreach ( var v in abilities )
			{
				var ability = Item.GetAbility( v );

				if ( ability != null && ability.HasDependencies() && ability.IsAvailable() )
				{
					var button = AddChild<ItemCommandAbility>( "command" );
					button.Update( Item, ability );
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
		public Label Damage { get; private set; }
		public ISelectable Item { get; private set; }
		public ItemQueueList QueueList { get; private set; }
		public ItemOccupantList OccupantList { get; private set; }


		public ItemInformation()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );
			Health = Add.Label( "", "health" );
			Damage = Add.Label( "", "damage" );
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

			Kills.SetClass( "hidden", true );
			Damage.SetClass( "hidden", true );

			if ( Item is UnitEntity unit )
			{
				Kills.Text = $"Kills: {unit.Kills}";
				Kills.SetClass( "hidden", false );

				if ( unit.Weapon.IsValid() )
				{
					if ( unit.Rank != null )
						Damage.Text = $"Damage: {unit.Weapon.BaseDamage} (+{unit.Rank.DamageModifier})";
					else
						Damage.Text = $"Damage: {unit.Weapon.BaseDamage}";

					Damage.SetClass( "hidden", false );
				}
			}

			Health.Text = $"{Item.Health.CeilToInt()} HP";

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
			OccupantList.Update( entity );
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

		public void Update( IList<Entity> entities )
		{
			Update( entities.Cast<ISelectable>().ToList() );
		}

		public override void Tick()
		{
			if ( RTS.Gamemode.Instance == null ) return;

			var isHidden = true;

			if ( Hud.IsLocalPlaying() )
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
