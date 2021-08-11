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
			if ( IsDisabled ) return;

			if ( Selectable.IsUsingAbility() )
			{
				// We can't use another while once is being used.
				Audio.Play( "rts.beepvibrato" );
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

			Audio.Play( "rts.pophappy" );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = Hud.Tooltip;
			tooltip.Update( Ability, IsDisabled ) ;
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
			if ( IsDisabled ) return;

			var player = (Local.Pawn as Player);
			var status = Item.CanCreate( player, Selectable );

			if ( status != RequirementError.Success )
			{
				Audio.Play( status );
				return;
			}

			if ( Selectable is UnitEntity builder && Item is BaseBuilding building )
				Items.CreateGhost( builder, building );
			else
				Items.Queue( Selectable.NetworkIdent, Item.NetworkId );

			Audio.Play( "rts.pophappy" );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			var tooltip = Hud.Tooltip;
			tooltip.Update( Item, false, IsDisabled );
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
		public bool IsDisabled { get; private set; }

		public ItemCommand() : base()
		{

		}

		public void Disable()
		{
			AddClass( "disabled" );
			IsDisabled = true;
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
			if ( !Unit.IsValid() )
			{
				Audio.Play( "rts.beepvibrato" );
				return;
			}

			Items.Evict( Occupiable.NetworkIdent, Unit.NetworkIdent );

			Audio.Play( "rts.pophappy" );
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
			Audio.Play( "rts.pophappy" );
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
		public ISelectable Selectable { get; private set; }
		public List<ItemCommand> Buttons { get; private set; }

		public ItemCommandList()
		{
			Buttons = new();
		}

		public void Update( ISelectable selectable )
		{
			Selectable = selectable;

			Buttons.ForEach( b => b.Delete( true ) );
			Buttons.Clear();

			if ( selectable is UnitEntity unit )
				UpdateCommands( unit.Item.Queueables, unit.Item.Abilities );
			else if ( selectable is BuildingEntity building )
				UpdateCommands( building.Item.Queueables, building.Item.Abilities );
		}

		private void UpdateCommands( HashSet<string> queueables, HashSet<string> abilities = null )
		{
			var player = Local.Pawn as Player;

			foreach ( var v in queueables )
			{
				var queueable = Items.Find<BaseItem>( v );

				if ( queueable == null )
				{
					Log.Error( "[SelectedItem::UpdateCommands] Unable to find queueable by name: " + v );
					return;
				}

				var button = AddChild<ItemCommandQueueable>( "command" );

				if ( !queueable.CanHave( player, Selectable ) )
					button.Disable();

				button.Update( Selectable, queueable );

				Buttons.Add( button );
			}

			if ( abilities == null ) return;

			foreach ( var v in abilities )
			{
				var ability = Selectable.GetAbility( v );

				if ( ability != null && ability.IsAvailable() )
				{
					var button = AddChild<ItemCommandAbility>( "command" );

					if ( !ability.HasDependencies() )
						button.Disable();

					button.Update( Selectable, ability );
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
		public ISelectable Selectable { get; private set; }
		public ItemQueueList QueueList { get; private set; }
		public ResistanceValues Resistances { get; private set; }
		public ItemOccupantList OccupantList { get; private set; }


		public ItemInformation()
		{
			Name = Add.Label( "", "name" );
			Desc = Add.Label( "", "desc" );
			Health = Add.Label( "", "health" );
			Damage = Add.Label( "", "damage" );
			Kills = Add.Label( "", "kills" );
			Resistances = AddChild<ResistanceValues>( "resistances" );
			QueueList = AddChild<ItemQueueList>();
			OccupantList = AddChild<ItemOccupantList>();

			foreach ( var kv in RTS.Resistances.Table )
			{
				Resistances.AddResistance( kv.Value );
			}
		}

		public void Update( ISelectable selectable )
		{
			Selectable = selectable;

			if ( selectable is UnitEntity unit )
				UpdateUnit( unit );
			else if ( selectable is BuildingEntity building )
				UpdateBuilding( building );
		}

		public override void Tick()
		{
			if ( Selectable == null ) return;

			Kills.SetClass( "hidden", true );
			Damage.SetClass( "hidden", true );

			if ( Selectable is UnitEntity unit )
			{
				if ( unit.Weapon.IsValid() )
				{
					Kills.Text = $"Kills: {unit.Kills}";
					Kills.SetClass( "hidden", false );

					var baseDamage = unit.Weapon.BaseDamage;
					var fullDamage = unit.Weapon.GetDamage();
					var difference = fullDamage - baseDamage;
					var perSecond = unit.Weapon.GetDamagePerSecond();

					if ( difference > 0 )
						Damage.Text = $"Damage: {baseDamage}+{difference} ({perSecond} DPS)";
					else
						Damage.Text = $"Damage: {baseDamage} ({perSecond} DPS)";

					Damage.SetClass( "hidden", false );
				}
			}

			Health.Text = $"{Selectable.Health.CeilToInt()} HP";

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

			var resistances = entity.Item.Resistances;

			Resistances.SetVisible( resistances.Count > 0 );

			foreach ( var kv in Resistances.Values )
			{
				if ( resistances.TryGetValue( kv.Key, out var resistance ) )
				{
					kv.Value.Update( resistance );
					kv.Value.SetVisible( true );
				}
				else
				{
					kv.Value.SetVisible( false );
				}
			}
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

			var resistances = entity.Modifiers.Resistances;

			Resistances.SetVisible( resistances.Count > 0 );

			foreach ( var kv in Resistances.Values )
			{
				if ( resistances.TryGetValue( kv.Key, out var resistance ) )
				{
					kv.Value.Update( resistance );
					kv.Value.SetVisible( true );
				}
				else
				{
					kv.Value.SetVisible( false );
				}
			}
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
			if ( Gamemode.Instance == null ) return;

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
