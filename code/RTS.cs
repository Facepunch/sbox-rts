using Gamelib.FlowField;
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamelib.Math;

namespace Facepunch.RTS
{
	[Library( "rts", Title = "RTS" )]
	public partial class RTS : Game
	{
		public struct CameraConfig
		{
			public bool Ortho;
			public float PanSpeed;
			public float ZoomScale;
			public float FOV;
			public float Backward;
			public float Left;
			public float Up;
		}

		public struct GameConfig
		{
			public CameraConfig Camera;
		}

		public static ItemTooltip Tooltip => ItemTooltip.Instance;
		public static SoundManager Sound => SoundManager.Instance;
		public static ItemManager Item => ItemManager.Instance;
		public static FogManager Fog => FogManager.Instance;
		public static RTS Game { get; private set; }

		[Net] public float ServerTime { get; private set; }
		[Net] public BaseRound Round { get; private set; }
		[Net] public GameConfig Config { get; private set; }

		public Dictionary<ulong, int> Ratings { get; private set; }
		public BaseRound LastRound { get; private set; }
		public Pathfinder Pathfinder { get; private set; }

		public RTS()
		{
			if ( IsServer )
			{
				LoadConfig();
				LoadRatings();

				_ = new SoundManager();
				_ = new ItemManager();
				_ = new FogManager();
				_ = new Hud();
			}

			Game = this;
		}

		public void ToastAll( string text, string icon = "" )
		{
			Toast( To.Everyone, text, icon );
		}

		public void Toast( Player player, string text, string icon = "" )
		{
			Toast( To.Single( player ), text, icon );
		}
		
		[ClientRpc]
		public void Toast( string text, string icon = "" )
		{
			ToastList.Instance.AddItem( text, Texture.Load( icon ) );
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

			Pathfinder = new Pathfinder();

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
				LoadConfig();
			}
		}

		private void LoadConfig()
		{
			// TODO: Decals don't render in orthographic for some reason.

			/*
			Config = new GameConfig
			{
				Camera = new CameraConfig
				{
					Ortho = true,
					PanSpeed = 5000f,
					ZoomScale = 2f,
					Backward = 3000f,
					Left = 3000f,
					Up = 4000f
				}
			};
			*/

			Config = new GameConfig
			{
				Camera = new CameraConfig
				{
					Ortho = false,
					PanSpeed = 5000f,
					ZoomScale = 0.6f,
					FOV = 30f,
					Backward = 2500f,
					Left = 2500f,
					Up = 5000f
				}
			};
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
