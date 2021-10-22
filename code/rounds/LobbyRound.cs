using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
    public class LobbyRound : BaseRound
	{
		public override string RoundName => "LOBBY";
		public override bool ShowRoundInfo => true;

		private RealTimeUntil StartGameTime { get; set; }
		private bool AllPlayersReady { get; set; }

		public void UpdateReadyState()
		{
			var allPlayersReady = true;
			var teamGroups = new HashSet<int>();

			foreach ( var player in Players )
			{
				if ( !player.IsReady )
				{
					allPlayersReady = false;
				}

				teamGroups.Add( player.TeamGroup );
			}

			if ( allPlayersReady )
			{
				if ( !AllPlayersReady )
				{
					if ( teamGroups.Count == 1 )
					{
						Hud.ToastAll( "There must be at least two different team groups to play!" );
						return;
					}

					Hud.ToastAll( "All players are ready. Starting game in 5 seconds..." );

					AllPlayersReady = true;
					StartGameTime = 5f;
				}
			}
			else if ( AllPlayersReady )
			{
				Hud.ToastAll( "The game will no longer start until all players are ready." );

				AllPlayersReady = false;
			}
		}

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				var players = Client.All.Select( ( client ) => client.Pawn as Player );

				foreach ( var player in players )
					OnPlayerJoin( player );
			}
			else
			{
				LobbyDialog.Show();
			}
		}

		protected override void OnFinish()
		{
			if ( Host.IsClient )
			{
				LobbyDialog.Close();
			}

			base.OnFinish();
		}

		public override void OnSecond()
		{
			if ( AllPlayersReady && StartGameTime )
			{
				Rounds.Change( new PlayRound() );
			}

			base.OnSecond();
		}

		public override void OnPlayerJoin( Player player )
		{
			if ( Players.Contains( player ) )
			{
				return;
			}

			var colors = new List<Color>
			{
				Color.Red,
				Color.Blue,
				Color.Green,
				Color.Cyan,
				Color.Magenta,
				Color.Orange,
				Color.Yellow
			};

			foreach ( var other in Players )
			{
				colors.Remove( other.TeamColor );
			}

			var client = player.Client;
			var clientEntity = (Entity)player.Client;

			player.TeamColor = Rand.FromList( colors );
			player.TeamGroup = clientEntity.NetworkIdent;

			// This is a temporary hack.
			var delta = client.SteamId - 90071996842377216;
			var isBot = (delta >= 0 && delta <= 64);

			player.IsReady = isBot;

			player.MakeSpectator( true );

			AddPlayer( player );

			base.OnPlayerJoin( player );
		}
	}
}
