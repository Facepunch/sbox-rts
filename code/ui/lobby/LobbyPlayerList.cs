
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	public class LobbyColorButton : Panel
	{
		public RTSPlayer Player { get; set; }

		protected override void OnClick( MousePanelEvent e )
		{
			if ( Player.IsValid() && Player.IsLocalPawn && !Player.IsReady )
			{
				LobbyDialog.SetNextTeamGroup();
			}

			base.OnClick( e );
		}
	}

	public class LobbyPlayerItem : Panel
	{
		public LobbyColorButton TeamColor { get; private set; }
		public Label TeamGroup { get; private set; }
		public Image Avatar { get; private set; }
		public Label Name { get; private set; }

		public LobbyPlayerItem()
		{
			Avatar = Add.Image( "", "avatar" );
			TeamColor = AddChild<LobbyColorButton>( "color" );
			TeamGroup = TeamColor.Add.Label( "0", "group" );
			Name = Add.Label( "", "name" );
		}

		public void Update( RTSPlayer player )
		{
			if ( player == null )
			{
				SetClass( "hidden", true );
				SetClass( "ready", false );

				return;
			}

			var client = player.Client;

			Avatar.SetTexture( $"avatar:{client.SteamId}" );

			TeamGroup.Text = player.TeamGroup.ToString();
			Name.Text = client.Name;

			TeamColor.Style.BackgroundColor = player.TeamColor;
			TeamColor.Style.Dirty();

			TeamColor.Player = player;
			TeamColor.SetClass( "enabled", !player.IsReady && player.IsLocalPawn );

			SetClass( "hidden", false );
			SetClass( "ready", player.IsReady );
		}

		public override void Tick()
		{
			base.Tick();
		}
	}

	public class LobbyPlayerList : Panel
	{
		public List<LobbyPlayerItem> Players { get; private set; }

		public LobbyPlayerList()
		{
			StyleSheet.Load( "/ui/lobby/LobbyPlayerList.scss" );

			Players = new();

			for ( var i = 0; i < 4; i++ )
			{
				var item = AddChild<LobbyPlayerItem>( "player" );
				Players.Add( item );
			}
		}

		public override void Tick()
		{
			for ( var i = 0; i < 4; i++ )
			{
				if ( Entity.FindByIndex( i + 1 ) is Client player )
					Players[i].Update( player.Pawn as RTSPlayer );
				else
					Players[i].Update( null );
			}

			base.Tick();
		}
	}
}
