
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class MiniMap : Panel
	{
		public Scene Scene;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			/*
			Capture = SceneCapture.Create( "minimap", 1024, 1024 );
			Capture.World = SceneWorld.Current;
			*/

			//Scene = Add.Scene( SceneWorld.Current, Vector3.Up * 1000f, Vector3.VectorAngle( Vector3.Down ), 70f, "scene" );
		}

		public override void Tick()
		{
			SetClass( "hidden", true);

			var player = Local.Pawn as Player;
			if ( player == null || player.IsSpectator ) return;

			var game = RTS.Game;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			if ( round is PlayRound )
				SetClass( "hidden", false );
		}
	}
}
