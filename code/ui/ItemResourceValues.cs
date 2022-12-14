
using Gamelib.Extensions;
using Facepunch.RTS.Units;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/ItemResourceValues.scss" )]
	public class ItemResourceValues : Panel
	{
		public Dictionary<ResourceType, ItemResourceValue> Values { get; private set; }

		public ItemResourceValues()
		{
			Values = new();
		}

		public void AddResource( ResourceType type )
		{
			var panel = AddChild<ItemResourceValue>();
			panel.Update( type, 0 );
			Values.Add( type, panel );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
