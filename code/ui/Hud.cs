
using Sandbox;
using Sandbox.UI;

namespace Facepunch.RTS
{
	[Library]
	public partial class Hud : HudEntity<RootPanel>
	{
		public Panel Header { get; private set; }
		public Panel Footer { get; private set; }

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<CursorController>();
			RootPanel.AddChild<EntityHud>();
			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<MiniMap>();
			RootPanel.AddChild<ResourceList>();
			RootPanel.AddChild<PopulationCount>();
			RootPanel.AddChild<SelectedItem>();
			RootPanel.AddChild<ItemTooltip>();
			RootPanel.AddChild<VoiceList>();

			var chatBox = RootPanel.AddChild<ChatBox>();
			chatBox.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<ToastList>();
			RootPanel.AddChild<LoadingScreen>();
		}
	}
}
