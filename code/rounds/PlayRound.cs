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

		public override void OnPlayerJoin( Player player )
		{
			player.MakeSpectator( true );
			Spectators.Add( player );

			base.OnPlayerJoin( player );
		}
		
		protected override void OnStart()
		{
			if ( Host.IsServer )
			{
				var spawnpoints = Entity.All.OfType<SpawnPoint>().ToList().Shuffle();
				var players = Client.All.Select( ( client ) => client.Pawn as Player ).ToList();
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

				foreach ( var player in players )
				{
					if ( spawnpoints.Count > 0 )
					{
						player.TeamColor = colors[0];
						colors.RemoveAt( 0 );

						var spawnpoint = spawnpoints[0];
						spawnpoints.RemoveAt( 0 );

						var worker = Items.Create<UnitEntity>( player, "unit.worker" );
						worker.Position = spawnpoint.Position + (Vector3.Random * 20f).WithZ( spawnpoint.Position.z );
						player.AddPopulation( worker.Item.Population );

						player.SetResource( ResourceType.Stone, 1000 );
						player.SetResource( ResourceType.Metal, 500 );
						player.SetResource( ResourceType.Beer, 200 );
						player.SetResource( ResourceType.Plasma, 0 );


						player.SetResource( ResourceType.Stone, 5000 );
						player.SetResource( ResourceType.Metal, 5000 );
						player.SetResource( ResourceType.Beer, 2000 );
						player.SetResource( ResourceType.Plasma, 500 );

						player.MakeSpectator( false );
						player.LookAt( spawnpoint );

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
