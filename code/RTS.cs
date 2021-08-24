using Facepunch.RTS.Tech;
using Facepunch.RTS.Units;
using Gamelib.Extensions;
using Gamelib.FlowFields;
using Gamelib.FlowFields.Entities;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.RTS
{
	[Library( "rts", Title = "RTS" )]
	public partial class Gamemode : Game
	{
		public static Gamemode Instance { get; private set; }

		[Net] public float ServerTime { get; private set; }
		public BBox WorldSize { get; private set; } = new BBox( Vector3.One * -5000f, Vector3.One * 5000f );

		public Dictionary<ulong, int> Ratings { get; private set; }

		[ServerCmd("rts_kill")]
		public static void KillSelected()
		{
			if ( ConsoleSystem.Caller.Pawn is Player caller )
			{
				caller.ForEachSelected<UnitEntity>( ( unit ) =>
				{
					unit.TakeDamage( DamageInfo.Generic( unit.Health * 2f ) );
					return false;
				} );
			}
		}

		[ServerCmd( "rts_learnit" )]
		public static void LearnTechnology( string technology )
		{
			if ( ConsoleSystem.Caller.Pawn is Player caller )
			{
				var item = Items.Find<BaseTech>( technology );

				caller.AddDependency( item );
				item.OnCreated( caller, null );
			}
		}

		[ServerCmd( "rts_richboy" )]
		public static void GiveAllResources()
		{
			if ( ConsoleSystem.Caller.Pawn is Player caller )
			{
				caller.SetResource( ResourceType.Stone, 9999 );
				caller.SetResource( ResourceType.Metal, 9999 );
				caller.SetResource( ResourceType.Beer, 9999 );
				caller.SetResource( ResourceType.Plasma, 9999 );
			}
		}

		public Gamemode()
		{
			if ( IsServer )
			{
				LoadRatings();

				_ = new Hud();
			}

			Ranks.Initialize();
			Items.Initialize();
			Resistances.Initialize();

			if ( FlowFieldGround.Exists )
				WorldSize = FlowFieldGround.Bounds;

			if ( IsClient )
			{
				Fog.Initialize( WorldSize );
			}

			if ( IsServer )
			{
				// TODO: Remove this. Temporarily add and remove some tags to add them to the string pool.
				// This is because the client will throw an exception when using tags with traces and no
				// entity exists yet with those tags.

				Tags.Add( "blueprint" );
				Tags.Remove( "blueprint" );
			}

			FlowFieldGround.OnUpdated += OnGroundUpdated;

			Instance = this;
		}

		public void UpdateRating( Player player )
		{
			var client = player.GetClientOwner();
			Ratings[client.SteamId] = player.Elo.Rating;
		}

		public override void DoPlayerNoclip( Client client )
		{
			// Do nothing. The player can't noclip in this mode.
		}

		public override void DoPlayerSuicide( Client client )
		{
			// Do nothing. The player can't suicide in this mode.
		}

		public override void OnKilled( Entity entity )
		{
			// Do nothing. The player cannot be killed in this mode.
		}

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			Rounds.Current?.OnPlayerLeave( client.Pawn as Player );

			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player();

			if ( Ratings.TryGetValue( client.SteamId, out var rating ) )
				player.Elo.Rating = rating;

			client.Pawn = player;

			Rounds.Current?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		[Event.Entity.PostSpawn]
		private void OnEntityPostSpawn()
		{
			if ( IsServer )
			{
				_ = StartSecondTimer();

				var units = Items.List.OfType<BaseUnit>();

				PathManager.SetBounds( WorldSize );

				foreach ( var unit in units )
				{
					var collisionSize = unit.CollisionSize;
					var nodeSize = unit.NodeSize;

					PathManager.Create( nodeSize, collisionSize );
				}
			}
			else
			{
				_ = StartFogUpdater();
			}
		}

		private void OnGroundUpdated()
		{
			WorldSize = FlowFieldGround.Bounds;

			if ( IsClient )
				Fog.UpdateSize( WorldSize );
		}

		private async Task StartSecondTimer()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		private async Task StartFogUpdater()
		{
			while ( true )
			{
				await Task.Delay( 100 );
				Fog.Update();
			}
		}

		private void OnSecond()
		{
			CheckMinimumPlayers();
		}

		[Event.Tick]
		private void Tick()
		{
			if ( IsServer )
			{
				PathManager.Update();
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
				if ( Rounds.Current is LobbyRound || Rounds.Current == null )
				{
					Rounds.Change( new PlayRound() );
				}
			}
			else if ( Rounds.Current is not LobbyRound )
			{
				Rounds.Change( new LobbyRound() );
			}
		}
	}
}
