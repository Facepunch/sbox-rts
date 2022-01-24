using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class SelectionGroup : Panel
	{
		private class SelectableAndImage
		{
			public ISelectable Selectable;
			public Panel Health;
			public Image Icon;
		}

		private Label Slot;
		private Panel Container;
		private List<SelectableAndImage> List;
		private RealTimeUntil NextUpdateTime;

		public SelectionGroup()
		{
			Slot = Add.Label( "", "slot" );
			Container = Add.Panel( "icons" );
			List = new();
		}

		public void SetSlot( int slot )
		{
			Slot.Text = $"{slot}.";
		}

		public List<ISelectable> GetSelectables()
		{
			var output = new List<ISelectable>();

			for ( var i = 0; i < List.Count; i++ )
			{
				output.Add( List[i].Selectable );
			}

			return output;
		}

		public void Clear()
		{
			Container.DeleteChildren();
			List.Clear();
		}

		public void AddSelectable( ISelectable selectable )
		{
			var item = Container.Add.Panel( "item" );

			var icon = item.Add.Image( null, "icon" );
			icon.Texture = selectable.GetBaseItem().Icon;

			List.Add( new SelectableAndImage()
			{
				Health = item.Add.Panel( "health" ),
				Icon = icon,
				Selectable = selectable
			} );
		}

		public override void Tick()
		{
			if ( NextUpdateTime )
			{
				for ( var i = List.Count - 1; i >= 0; i-- )
				{
					var data = List[i];
					var entity = (data.Selectable as Entity);

					if ( !entity.IsValid() )
					{
						data.Icon.Delete();
						List.RemoveAt( i );
						continue;
					}

					var fraction = Length.Fraction( 1f - (data.Selectable.Health / data.Selectable.MaxHealth) );

					if ( data.Health.Style.Width != fraction )
					{
						data.Health.Style.Width = fraction;
						data.Health.Style.Dirty();
					}
				}

				NextUpdateTime = 0.5f;
			}

			SetClass( "hidden", List.Count == 0 );

			base.Tick();
		}
	}

	public class SelectionGroups : Panel
	{
		private static SelectionGroups Instance { get; set; }

		public SelectionGroup[] Groups { get; private set; } = new SelectionGroup[9];

		public Panel Container { get; private set; }

		public static void Update( int slot, List<ISelectable> selectables )
		{
			var group = Instance.Groups[slot - 1];

			group.Clear();

			for ( var i = 0; i < selectables.Count; i++ )
			{
				group.AddSelectable( selectables[i] );
			}
		}

		public static List<ISelectable> GetInSlot( int slot )
		{
			var group = Instance.Groups[slot - 1];
			return group.GetSelectables();
		}

		public SelectionGroups()
		{
			StyleSheet.Load( "/ui/SelectionGroups.scss" );

			Container = Add.Panel( "container" );

			for ( var i = 0; i < Groups.Length; i++ )
			{
				var group = Container.AddChild<SelectionGroup>();
				group.SetSlot( i + 1 );
				Groups[i] = group;
			}

			Instance = this;
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );

			base.Tick();
		}
	}
}
