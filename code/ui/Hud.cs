
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

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

			RootPanel.AddChild<RoundInfo>();
			RootPanel.AddChild<VoiceList>();
			RootPanel.AddChild<ChatBox>();
			RootPanel.AddChild<LoadingScreen>();
			RootPanel.AddChild<CursorController>();
		}
	}
}
