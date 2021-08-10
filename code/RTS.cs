using Facepunch.RTS.Units;
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
		public float DefaultWorldSize { get; private set; } = 10000f;

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

			if ( IsClient )
			{
				Fog.Initialize();
			}

			Instance = this;
		}

		public void UpdateRating( Player player )
		{
			var client = player.GetClientOwner();
			Ratings[client.SteamId] = player.Elo.Rating;
		}

		public float GetWorldSize()
		{
			if ( FlowFieldGround.Exists )
				return FlowFieldGround.Bounds.Size.x;
			else
				return 0f;
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

			if ( IsServer )
			{
				var units = Items.List.OfType<BaseUnit>();

				if ( FlowFieldGround.Exists )
					PathManager.SetBounds( FlowFieldGround.Bounds );
				else
					PathManager.SetBounds( new BBox( Vector3.One * -DefaultWorldSize, Vector3.One * DefaultWorldSize ) );

				foreach ( var unit in units )
				{
					var collisionSize = unit.CollisionSize;
					var nodeSize = unit.NodeSize;

					PathManager.Create( nodeSize, collisionSize );
				}
			}

			base.PostLevelLoaded();
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
