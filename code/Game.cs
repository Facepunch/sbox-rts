using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RTS
{
	[Library( "rts", Title = "RTS" )]
	partial class Game : Sandbox.Game
	{
		public Hud Hud { get; set; }

		public static Game Instance
		{
			get => Current as Game;
		}

		[Net] public BaseRound Round { get; private set; }
		
		private Dictionary<ulong, int> _ratings;
		private BaseRound _lastRound;

		[ServerVar( "rts_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		public Game()
		{
			if ( IsServer )
			{
				LoadRatings();
				Hud = new();
			}
		}
		
		public void UpdateRating( Player player )
		{
			var client = player.GetClientOwner();
			_ratings[client.SteamId] = player.Elo.Rating;
		}

		public void SaveRatings()
		{
			//FileSystem.Mounted.WriteAllText( "data/rts/ratings.json", JsonSerializer.Serialize( _ratings ) );
		}

		public void ChangeRound(BaseRound round)
		{
			Assert.NotNull( round );

			Round?.Finish();
			Round = round;
			Round?.Start();
		}

		public async Task StartSecondTimer()
		{
			while (true)
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		public override void DoPlayerNoclip( Client client )
		{
			// Do nothing. The player can't noclip in this mode.
		}

		public override void DoPlayerSuicide( Client client )
		{
			if ( client.Pawn.LifeState == LifeState.Alive && Round?.CanPlayerSuicide == true )
			{
				// This simulates the player being killed.
				client.Pawn.LifeState = LifeState.Dead;
				client.Pawn.OnKilled();
				OnKilled( client.Pawn );
			}
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();
			
			base.PostLevelLoaded();
		}

		public override void OnKilled( Entity entity )
		{
			if ( entity is Player player )
				Round?.OnPlayerKilled( player );

			base.OnKilled( entity );
		}

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			Round?.OnPlayerLeave( client.Pawn as Player );

			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player();

			if ( _ratings.TryGetValue( client.SteamId, out var rating ) )
				player.Elo.Rating = rating;

			client.Pawn = player;

			Round?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}
		
		private void OnSecond()
		{
			CheckMinimumPlayers();
			Round?.OnSecond();
		}

		[Event.Tick]
		private void Tick()
		{
			Round?.OnTick();
			
			if ( IsClient )
			{
				// We have to hack around this for now until we can detect changes in net variables.
				if ( _lastRound != Round )
				{
					_lastRound?.Finish();
					_lastRound = Round;
					_lastRound.Start();
				}
			}
		}

		private void LoadRatings()
		{
			_ratings = FileSystem.Mounted.ReadJsonOrDefault<Dictionary<ulong, int>>( "data/rts/ratings.json" ) ?? new();
		}

		private void CheckMinimumPlayers()
		{
			if ( Client.All.Count >= MinPlayers)
			{
				if ( Round is LobbyRound || Round == null )
				{
					ChangeRound( new PlayRound() );
				}
			}
			else if ( Round is not LobbyRound )
			{
				ChangeRound( new LobbyRound() );
			}
		}
	}
}
