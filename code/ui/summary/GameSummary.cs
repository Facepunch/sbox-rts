
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	public class GameSummaryWinner : Panel
	{
		public Panel TeamColor { get; private set; }
		public Label TeamGroup { get; private set; }
		public Image Avatar { get; private set; }
		public Label Name { get; private set; }

		public GameSummaryWinner()
		{
			Avatar = Add.Image( "", "avatar" );
			TeamColor = AddChild<LobbyColorButton>( "color" );
			TeamGroup = TeamColor.Add.Label( "0", "group" );
			Name = Add.Label( "", "name" );
		}

		public void Update( RTSPlayer player )
		{
			var client = player.Client;

			Avatar.SetTexture( $"avatar:{client.SteamId}" );

			TeamGroup.Text = player.TeamGroup.ToString();
			Name.Text = client.Name;

			TeamColor.Style.BackgroundColor = player.TeamColor;
			TeamColor.Style.Dirty();
		}

		public override void Tick()
		{
			base.Tick();
		}
	}

	public partial class GameSummary : Panel
	{
		public static GameSummary Instance { get; private set; }

		[ClientRpc]
		public static void Show( int teamGroup )
		{
			Close();

			Instance = Local.Hud.AddChild<GameSummary>();
			Instance.SetWinners( teamGroup );
		}

		[ClientRpc]
		public static void Close()
		{
			if ( Instance != null )
			{
				Instance.Delete( true );
				Instance = null;
			}
		}

		public Label Title { get; private set; }
		public Label WinnersTitle { get; private set; }
		public Panel WinnersList { get; private set; }

		public GameSummary()
		{
			StyleSheet.Load( "/ui/summary/GameSummary.scss" );

			Title = Add.Label( "GAME SUMMARY", "title" );
			WinnersList = Add.Panel( "winners" );
			WinnersTitle = WinnersList.Add.Label( "WINNERS", "title" );

			Hud.ChatBox.Parent = this;
		}

		public void SetWinners( int teamGroup )
		{
			var winners = Entity.All
				.OfType<RTSPlayer>()
				.Where( p => p.TeamGroup == teamGroup );

			foreach ( var winner in winners )
			{
				var item = WinnersList.AddChild<GameSummaryWinner>( "item" );
				item.Update( winner );
			}
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
