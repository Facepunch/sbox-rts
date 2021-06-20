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

		public static SoundManager Sound => SoundManager.Instance;
		public static ItemManager Item => ItemManager.Instance;
		public static FogManager Fog => FogManager.Instance;

		[Net] public float ServerTime { get; private set; }
		[Net] public BaseRound Round { get; private set; }

		public Dictionary<ulong, int> Ratings { get; private set; }
		public BaseRound LastRound { get; private set; }

		[ServerCmd("rts_test")]
		public static void Test()
		{
			if ( ConsoleSystem.Caller.Pawn is Player caller )
			{
				caller.GiveResource( ResourceType.Stone, Rand.Int( -1000, 1000 ) );
			}
		}

		public Game()
		{
			if ( IsServer )
			{
				LoadRatings();

				_ = new SoundManager();
				_ = new ItemManager();
				_ = new FogManager();
				_ = new Hud();
			}
		}

		public void UpdateRating( Player player )
		{
			var client = player.GetClientOwner();
			Ratings[client.SteamId] = player.Elo.Rating;
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

			if ( Ratings.TryGetValue( client.SteamId, out var rating ) )
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
				if ( LastRound != Round )
				{
					LastRound?.Finish();
					LastRound = Round;
					LastRound.Start();
				}
			}
			else
			{
				ServerTime = Time.Now;
			}
		}

		private void LoadRatings()
		{
			Ratings = FileSystem.Mounted.ReadJsonOrDefault<Dictionary<ulong, int>>( "data/rts/ratings.json" ) ?? new();
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
