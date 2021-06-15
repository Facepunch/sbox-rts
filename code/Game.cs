using RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RTS
{
	[Library( "rts", Title = "RTS" )]
	partial class Game : Sandbox.Game
	{
		public static Game Instance
		{
			get => Current as Game;
		}

		[Net] public float ServerTime { get; private set; }
		[Net] public BaseRound Round { get; private set; }

		private Dictionary<ulong, int> _ratings;
		private BaseRound _lastRound;

		public Game()
		{
			if ( IsServer )
			{
				LoadRatings();

				_ = new ItemManager();
				_ = new FogManager();
				_ = new Hud();
			}
		}

		public void UpdateRating( Player player )
		{
			var client = player.GetClientOwner();
			_ratings[client.SteamId] = player.Elo.Rating;
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
			// Do nothing. The player can't suicide in this mode.
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();
			
			base.PostLevelLoaded();
		}

		public override void OnKilled( Entity entity )
		{
			// Do nothing. The player cannot be killed in this mode.
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

				FogManager.Instance.Update();
			}
			else
			{
				ServerTime = Time.Now;
			}
		}

		private void LoadRatings()
		{
			_ratings = FileSystem.Mounted.ReadJsonOrDefault<Dictionary<ulong, int>>( "data/rts/ratings.json" ) ?? new();
		}

		private void CheckMinimumPlayers()
		{
			if ( Client.All.Count >= 2 )
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
