using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public class UnitGroup : Panel
	{
		private class UnitAndImage
		{
			public UnitEntity Unit;
			public Panel Health;
			public Image Icon;
		}

		private Label Slot;
		private Panel Container;
		private List<UnitAndImage> List;
		private RealTimeUntil NextUpdateTime;

		public UnitGroup()
		{
			Slot = Add.Label( "", "slot" );
			Container = Add.Panel( "icons" );
			List = new();
		}

		public void SetSlot( int slot )
		{
			Slot.Text = $"{slot}.";
		}

		public List<UnitEntity> GetUnits()
		{
			var output = new List<UnitEntity>();

			for ( var i = 0; i < List.Count; i++ )
			{
				output.Add( List[i].Unit );
			}

			return output;
		}

		public void Clear()
		{
			Container.DeleteChildren();
			List.Clear();
		}

		public void AddUnit( UnitEntity unit )
		{
			var icon = Container.Add.Image( null, "icon" );
			icon.Texture = unit.Item.Icon;

			List.Add( new UnitAndImage()
			{
				Health = icon.Add.Panel( "health" ),
				Icon = icon,
				Unit = unit
			} );
		}

		public override void Tick()
		{
			if ( NextUpdateTime )
			{
				for ( var i = List.Count - 1; i >= 0; i-- )
				{
					var data = List[i];

					if ( !data.Unit.IsValid() )
					{
						data.Icon.Delete();
						List.RemoveAt( i );
						continue;
					}

					var fraction = Length.Fraction( 1f - (data.Unit.Health / data.Unit.MaxHealth) );

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

	public class UnitGroups : Panel
	{
		private static UnitGroups Instance { get; set; }

		public UnitGroup[] Groups { get; private set; } = new UnitGroup[9];

		public Panel Container { get; private set; }

		public static void Update( int slot, List<UnitEntity> units )
		{
			var group = Instance.Groups[slot - 1];

			group.Clear();

			for ( var i = 0; i < units.Count; i++ )
			{
				group.AddUnit( units[i] );
			}
		}

		public static List<UnitEntity> GetUnits( int slot )
		{
			var group = Instance.Groups[slot - 1];
			return group.GetUnits();
		}

		public UnitGroups()
		{
			StyleSheet.Load( "/ui/UnitGroups.scss" );

			Container = Add.Panel( "container" );

			for ( var i = 0; i < Groups.Length; i++ )
			{
				var group = Container.AddChild<UnitGroup>();
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
