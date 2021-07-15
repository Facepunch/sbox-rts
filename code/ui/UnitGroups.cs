
using Sandbox;
using Sandbox.UI;

namespace Facepunch.RTS
{
	public class UnitGroup : Panel
	{

	}

	public class UnitGroups : Panel
	{
		public Scene Scene;

		public UnitGroups()
		{
			StyleSheet.Load( "/ui/UnitGroups.scss" );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );
		}
	}
}
