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

		[Net] public BaseRound Round { get; private set; }

		private Dictionary<string, BaseItem> _itemTable;
		private Dictionary<ulong, int> _ratings;
		private List<BaseItem> _itemList;

		private BaseRound _lastRound;

		[ServerCmd]
		public static void Attack( string id )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var targetId = Convert.ToInt32( id );
			var target = FindByIndex( targetId );

			if ( target.IsValid() )
			{
				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						unit.FollowTarget = true;
						unit.Target = target;
					}
				}
			}
		}
		
		[ServerCmd]
		public static void MoveToLocation( string csv )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			var entries = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => Convert.ToSingle( i ) )
				.ToList();

			if ( entries.Count == 3 )
			{
				var position = new Vector3( entries[0], entries[1], entries[2] );

				foreach ( var entity in caller.Selection )
				{
					if ( entity is UnitEntity unit )
					{
						unit.MoveTo( position );
					}
				}
			}
		}

		[ServerCmd]
		public static void SelectItems( string csv = null )
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;

			if ( !caller.IsValid() || caller.IsSpectator )
				return;

			caller.ClearSelection();

			if ( string.IsNullOrEmpty( csv ) )
				return;

			var entities = csv.Split( ',', StringSplitOptions.TrimEntries )
				.Select( i => FindByIndex( Convert.ToInt32( i ) ) );

			foreach ( var entity in entities )
			{
				if ( entity is not ISelectable selectable )
					continue;

				if ( caller.Selection.Count > 0 && !selectable.CanMultiSelect )
					continue;

				if ( selectable.Player == caller )
				{
					selectable.Select();
				}
			}
		}

		public Game()
		{
			if ( IsServer )
			{
				LoadRatings();
				_ = new Hud();
			}

			BuildItemTable();
		}

		public T FindItem<T>( string id ) where T : BaseItem
		{
			if ( _itemTable.TryGetValue( id, out var item ) )
				return (item as T);

			return null;
		}

		public T FindItem<T>( uint id ) where T : BaseItem
		{
			if ( id < _itemList.Count )
				return (_itemList[(int)id] as T);

			return null;
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

		private void BuildItemTable()
		{
			_itemTable = new();
			_itemList = new();

			var list = new List<BaseItem>();

			foreach ( var type in Library.GetAll<BaseItem>() )
			{
				var item = Library.Create<BaseItem>( type );
				list.Add( item );
			}

			// Sort alphabetically, this should result in the same index for client and server.
			list.Sort( ( a, b ) => a.UniqueId.CompareTo( b.UniqueId ) );

			for ( var i = 0; i < list.Count; i++ )
			{
				var item = list[i];

				_itemTable.Add( item.UniqueId, item );
				_itemList.Add( item );

				item.NetworkId = (uint)i;

				Log.Info( $"Adding {item.UniqueId} to the available items (id = {i})" );
			}
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
