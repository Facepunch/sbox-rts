
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.RTS
{
	public class MiniMapImage : Image
	{
		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Local.Pawn is Player player )
			{
				var size = Box.Rect.Size;
				var fractionX = (MousePosition.x / size.x);
				var fractionY = (MousePosition.y / size.y);
				var worldSize = Gamemode.Instance.WorldSize.Size;
				var largestSide = MathF.Max( worldSize.x, worldSize.y );
				var positionX = (largestSide * fractionX) - (largestSide * 0.5f);
				var positionY = (largestSide * fractionY) - (largestSide * 0.5f);

				player.Position = new Vector3( -positionY, -positionX );

				Player.LookAt( player.Position.ToCSV() );
			}
		}
	}

	public class MiniMap : Panel
	{
		public Image Map;
		public Texture ColorTexture;
		public Texture DepthTexture;
		public RealTimeUntil NextRender;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			Map = AddChild<MiniMapImage>( "map" );

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
			if ( Local.Pawn is Player player && NextRender )
			{
				var worldSize = Gamemode.Instance.WorldSize.Size;
				var cameraHeight = MathF.Max( worldSize.x, worldSize.y );
				var renderSize = new Vector2( 512f, 512f );
				var position = Vector3.Up * cameraHeight;
				var angles = Rotation.LookAt( Vector3.Down ).Angles();

				//Render.DrawScene( Map.Texture, DepthTexture, renderSize, SceneWorld.Current, position, angles, 50f, Color.White, Color.Black, 0.1f, cameraHeight );

				NextRender = 0.5f;
			}

			base.DrawBackground( ref state );
		}

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );
		}
	}
}
