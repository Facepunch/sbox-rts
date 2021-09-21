using Gamelib.Extensions;
using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch.RTS;

namespace Facepunch.RTS
{
    public partial class PlayRound : BaseRound
	{
		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool ShowTimeLeft => true;

		public List<Player> Spectators = new();

		private RealTimeUntil NextWinCheck { get; set; }

		public override void OnPlayerJoin( Player player )
		{
			player.MakeSpectator( true );
			Spectators.Add( player );

			base.OnPlayerJoin( player );
		}

		public override void OnSecond()
		{
			if ( NextWinCheck )
			{
				for ( int i = Players.Count - 1; i >= 0; i-- )
				{
					var player = Players[i];
					var workers = player.GetUnits<Worker>();
					var commandCentres = player.GetBuildings<CommandCentre>();

					if ( !workers.Any() && !commandCentres.Any() )
					{
						player.MakeSpectator( true );
						Spectators.Add( player );
						Players.Remove( player );
					}
				}

				var groups = Players.Select( p => p.TeamGroup ).Distinct();
				var count = groups.Count();

				if ( count == 1 )
				{
					GameSummary.Show( To.Everyone, groups.First() );
					Rounds.Change( new StatsRound() );
				}

				NextWinCheck = 5f;
			}

			base.OnSecond();
		}

		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				var spawnpoints = Entity.All.OfType<SpawnPoint>().ToList().Shuffle();
				var players = Client.All.Select( ( client ) => client.Pawn as Player ).ToList();

				foreach ( var player in players )
				{
					if ( spawnpoints.Count > 0 )
					{
						var spawnpoint = spawnpoints[0];
						spawnpoints.RemoveAt( 0 );

						var worker = Items.Create<UnitEntity>( player, "unit.worker" );
						worker.Position = spawnpoint.Position + (Vector3.Random * 20f).WithZ( spawnpoint.Position.z );
						player.AddPopulation( worker.Item.Population );

						player.SetResource( ResourceType.Stone, 1200 );
						player.SetResource( ResourceType.Metal, 600 );
						player.SetResource( ResourceType.Beer, 250 );
						player.SetResource( ResourceType.Plasma, 0 );

						player.MakeSpectator( false );
						player.LookAt( spawnpoint.Position );

						Fog.Show( player );
						Fog.Clear( player );
						Fog.MakeVisible( player, spawnpoint.Position, 2500f );

						AddPlayer( player );
					}
					else
					{
						Fog.Hide( player );

						player.MakeSpectator( true );
						Spectators.Add( player );
					}
				}
			}
		}

		protected override void OnFinish()
		{
			if ( Host.IsServer )
			{
				Spectators.Clear();
			}
		}
	}
}
