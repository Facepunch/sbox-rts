using Sandbox.UI;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/ResistanceValues.scss" )]
	public class ResistanceValues : Panel
	{
		public Dictionary<string, ResistanceValue> Values { get; private set; }

		public ResistanceValues()
		{
			Values = new();
		}

		public void AddResistance( BaseResistance resistance )
		{
			var panel = AddChild<ResistanceValue>();
			panel.Update( resistance, 0 );
			Values.Add( resistance.UniqueId, panel );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
