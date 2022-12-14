using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[StyleSheet( "/ui/DependencyList.scss" )]
	public class DependencyList : Panel
	{
		public Dictionary<string, DependencyValue> Dependencies { get; private set; }
		public Label Header { get; private set; }

		public DependencyList()
		{
			Header = Add.Label( "", "header" );

			Dependencies = new();
		}

		public void Clear()
		{
			foreach ( var kv in Dependencies )
			{
				kv.Value.Delete( true );
			}

			Dependencies.Clear();
		}

		public void AddDependency( BaseItem dependency )
		{
			var panel = AddChild<DependencyValue>();
			panel.Update( dependency );
			Dependencies.Add( dependency.UniqueId, panel );
		}

		public void SetVisible( bool isVisible )
		{
			SetClass( "hidden", !isVisible );
		}
	}
}
