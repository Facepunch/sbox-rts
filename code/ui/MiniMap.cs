
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace RTS
{
	public class MiniMap : Panel
	{
		public Image Image;
		public Texture Texture;
		public SceneCapture Capture;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			/*
			Capture = SceneCapture.Create( "minimap", 1024, 1024 );
			Capture.World = SceneWorld.Current;
			*/

			Image = Add.Image( "", "texture" );
		}

		public override void Tick()
		{
			SetClass( "hidden", true);

			var player = Local.Pawn as Player;
			if ( player == null || player.IsSpectator ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			if ( round is PlayRound )
				SetClass( "hidden", false );
		}
	}
}
