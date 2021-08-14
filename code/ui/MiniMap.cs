
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class MiniMap : Panel
	{
		public Image Map;
		public Texture ColorTexture;
		public Texture DepthTexture;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			Map = Add.Image( "", "map" );

			CreateTextureMap();
		}

		public void CreateTextureMap()
		{
			ColorTexture = Texture.CreateRenderTarget()
				.WithSize( 512, 512 )
				.Create();

			DepthTexture = Texture.CreateRenderTarget()
				.WithDepthFormat()
				.WithSize( 512, 512 )
				.Create();

			Map.Texture = ColorTexture;
		}

		public override void DrawBackground( ref RenderState state )
		{
			if ( Local.Pawn is Player player )
			{
				var renderSize = new Vector2( 512f, 512f );
				var position = player.Position + Vector3.Up * 10000f;
				var angles = Rotation.LookAt( Vector3.Down ).Angles();
				Render.DrawScene( Map.Texture, DepthTexture, renderSize, SceneWorld.Current, position, angles, 60f, Color.White, Color.Black, 0.1f, 20000f );
			}


			base.DrawBackground( ref state );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );
		}
	}
}
