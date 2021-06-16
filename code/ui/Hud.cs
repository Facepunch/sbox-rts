
using Sandbox;
using Sandbox.UI;

namespace RTS
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
			RootPanel.AddChild<ResourceDisplay>();
			RootPanel.AddChild<SelectedItem>();
			RootPanel.AddChild<ItemTooltip>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<LoadingScreen>();
		}
	}
}
