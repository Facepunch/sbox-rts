
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class LobbyDialog : Panel
	{
		public LobbyPlayerList PlayerList { get; private set; }
		public LobbyReadyButton ReadyButton { get; private set; }

		public static LobbyDialog Instance { get; private set; }

		public static LobbyDialog Show()
		{
			if ( Instance != null )
			{
				return Instance;
			}

			Instance = Local.Hud.AddChild<LobbyDialog>();

			return Instance;
		}

		[ConCmd.Server]
		public static void SetNextTeamGroup()
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				var nextTeamGroup = player.TeamGroup + 1;

				if ( nextTeamGroup > 4 )
					nextTeamGroup = 1;

				player.TeamGroup = nextTeamGroup;
			}
		}

		[ConCmd.Server]
		public static void SetReadyStatus( bool isReady )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				player.IsReady = isReady;

				if ( Rounds.Current is LobbyRound lobby )
				{
					lobby.UpdateReadyState();
				}
			}
		}

		public static void Close()
		{
			if ( Instance != null )
			{
				Instance.Delete();
				Instance = null;
			}
		}

		public LobbyDialog()
		{
			StyleSheet.Load( "/ui/lobby/LobbyDialog.scss" );

			PlayerList = AddChild<LobbyPlayerList>();
			ReadyButton = AddChild<LobbyReadyButton>();

			Hud.ChatBox.Parent = this;
		}

		public override void OnDeleted()
		{
			if ( Hud.ChatBox != null )
			{
				Hud.ChatBox.Parent = Local.Hud;
			}

			base.OnDeleted();
		}

		public override void Tick()
		{
			base.Tick();
		}
	}
}
