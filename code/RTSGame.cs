using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Entities;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
	public partial class RTSGame : GameManager
	{
		public static RTSGame Entity => Current as RTSGame;

		[Net] public float ServerTime { get; private set; }
		public BBox WorldSize { get; private set; } = new BBox( Vector3.One * -5000f, Vector3.One * 5000f );

		public Dictionary<long, int> Ratings { get; private set; }

		[ConCmd.Server("rts_kill")]
		public static void KillSelected()
		{
			if ( ConsoleSystem.Caller.Pawn is RTSPlayer caller )
			{
				caller.ForEachSelected<UnitEntity>( ( unit ) =>
				{
					unit.TakeDamage( DamageInfo.Generic( unit.Health * 2f ) );
					return false;
				} );
			}
		}

		[ConCmd.Server( "rts_doitnow" )]
		public static void SkipAllWaiting()
		{
			if ( ConsoleSystem.Caller.Pawn is RTSPlayer caller )
			{
				caller.SkipAllWaiting = true;
			}
		}

		[ConCmd.Server( "rts_learnit" )]
		public static void LearnTechnology( string technology )
		{
			if ( ConsoleSystem.Caller.Pawn is RTSPlayer caller )
			{
				var item = Items.Find<BaseTech>( technology );

				caller.AddDependency( item );
				item.OnCreated( caller, null );
			}
		}

		[ConCmd.Server( "rts_richboy" )]
		public static void GiveAllResources()
		{
			if ( ConsoleSystem.Caller.Pawn is RTSPlayer caller )
			{
				caller.SetResource( ResourceType.Stone, 9999 );
				caller.SetResource( ResourceType.Metal, 9999 );
				caller.SetResource( ResourceType.Beer, 9999 );
				caller.SetResource( ResourceType.Plasma, 9999 );
			}
		}

		public RTSGame() : base()
		{
			if ( Game.IsServer )
			{
				LoadRatings();

				_ = new Hud();
			}

			Ranks.Initialize();
			Items.Initialize();
			Resistances.Initialize();

			if ( FlowFieldGround.Exists )
				WorldSize = FlowFieldGround.Bounds;

			if ( Game.IsClient )
			{
				Fog.Initialize( WorldSize );
			}

			if ( Game.IsServer )
			{
				Game.TickRate = 20;
			}

			FlowFieldGround.OnUpdated += OnGroundUpdated;

			if ( Game.IsClient )
			{
				_ = StartFogUpdater();
			}
		}

		public void UpdateRating( RTSPlayer player )
		{
			var client = player.Client;
			Ratings[client.SteamId] = player.Elo.Rating;
		}

		public override void OnKilled( Entity entity )
		{
			// Do nothing. The player cannot be killed in this mode.
		}

		public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
		{
			Rounds.Current?.OnPlayerLeave( client.Pawn as RTSPlayer );

			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( IClient client )
		{
			var player = new RTSPlayer();

			if ( Ratings.TryGetValue( client.SteamId, out var rating ) )
				player.Elo.Rating = rating;

			client.Pawn = player;

			Rounds.Current?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		[Event.Entity.PostSpawn]
		private void OnEntityPostSpawn()
		{
			if ( Game.IsServer )
			{
				InitializePathManager();
			}
		}

		private async void InitializePathManager()
		{
			await GameTask.RunInThreadAsync( SetupPathCombinations );
			Rounds.Change( new LobbyRound() );
		}

		private async Task SetupPathCombinations()
		{
			PathManager.SetBounds( WorldSize );

			var combinations = new HashSet<Tuple<int, int>>();
			var units = Items.List.OfType<BaseUnit>();

			foreach ( var unit in units )
			{
				var collisionSize = unit.CollisionSize;
				var nodeSize = unit.NodeSize;

				combinations.Add( new Tuple<int, int>( nodeSize, collisionSize ) );
			}

			foreach ( var combination in combinations )
			{
				var collisionSize = combination.Item2;
				var nodeSize = combination.Item1;

				await PathManager.Create( nodeSize, collisionSize );
			}
		}

		private void OnGroundUpdated()
		{
			WorldSize = FlowFieldGround.Bounds;

			if ( Game.IsClient )
				Fog.UpdateSize( WorldSize );
		}

		private async Task StartFogUpdater()
		{
			while ( true )
			{
				try
				{
					await Task.Delay( 100 );
					Fog.Update();
				}
				catch ( TaskCanceledException )
				{
					break;
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}

		[Event.Tick]
		private void Tick()
		{
			if ( Game.IsServer )
			{
				PathManager.Update();
				ServerTime = Time.Now;
			}
		}

		private void LoadRatings()
		{
			Ratings = FileSystem.Mounted.ReadJsonOrDefault<Dictionary<long, int>>( "data/rts/ratings.json" ) ?? new();
		}
	}
}
