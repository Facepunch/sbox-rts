using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/ItemLabelValues.scss" )]
	public class ItemLabelValues : Panel
	{
		public List<ItemLabelValue> Values { get; private set; }

		public ItemLabelValues()
		{
			Values = new();
		}

		public void Clear()
		{
			DeleteChildren( true );
			Values.Clear();
		}

		public void AddItemLabel( ItemLabel data )
		{
			var panel = AddChild<ItemLabelValue>();
			panel.Update( data );
			Values.Add( panel );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
