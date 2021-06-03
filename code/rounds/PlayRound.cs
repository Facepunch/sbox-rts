using Gamelib.Extensions;
using RTS.Buildings;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS
{
    public partial class PlayRound : BaseRound
	{
		public override string RoundName => "PLAY";
		public override int RoundDuration => 0;
		public override bool ShowTimeLeft => true;

		public List<Player> Spectators = new();

		public override void OnPlayerJoin( Player player )
		{
			Spectators.Add( player );

			base.OnPlayerJoin( player );
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

						var b = new BuildingEntity();
						b.SetBuilding( Game.Instance.FindBuildable<BaseBuilding>( "building.headquarters" ) );
						b.RenderColor = player.TeamColor;
						b.Position = spawnpoint.Position;
						b.Player = player;

						player.MakeSpectator( false );
						player.LookAt( spawnpoint );

						AddPlayer( player );
					}
					else
					{
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
