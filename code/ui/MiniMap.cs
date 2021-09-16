
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

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

				player.LookAt( new Vector3( -positionY, -positionX ) );
			}
		}
	}

	public class MiniMapIcon : Panel
	{
		public IMapIconEntity Item { get; set; }

		public void Update()
		{
			if ( !Item.ShouldShowOnMap() )
			{
				SetClass( "hidden", true );
				return;
			}

			var worldSize = Gamemode.Instance.WorldSize.Size;
			var largestSide = MathF.Max( worldSize.x, worldSize.y );
			var position = Item.Position + (worldSize * 0.5f);
			var normalized = position / largestSide;

			Style.Left = Length.Fraction( normalized.x );
			Style.Top = Length.Fraction( normalized.y );
			Style.Dirty();

			SetClass( "hidden", false );
		}
	}

	public class MiniMap : Panel
	{
		public static MiniMap Instance { get; private set; }

		public readonly Panel IconContainer;
		public readonly Image Map;
		public readonly Panel Fog;

		private List<MiniMapIcon> Icons;
		private RealTimeUntil NextIconUpdate;

		//public Texture ColorTexture;
		//public Texture DepthTexture;
		//public RealTimeUntil NextRender;

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			Map = AddChild<MiniMapImage>( "map" );
			Map.SetTexture( "textures/rts/minimap/rts_greenlands.png" );

			Fog = Map.AddChild<Panel>( "fog" );

			RTS.Fog.OnActiveChanged += OnFogActiveChanged;

			IconContainer = Map.AddChild<Panel>( "icons" );

			Instance = this;
			Icons = new();

			//CreateTextureMap();
		}

		public void RemoveEntity( IMapIconEntity item )
		{
			var index = Icons.FindIndex( 0, Icons.Count, v => v.Item == item );

			if ( index >= 0 )
			{
				Icons[index].Delete();
				Icons.RemoveAt( index );
			}
		}

		public void AddEntity( IMapIconEntity item )
		{
			var icon = IconContainer.AddChild<MiniMapIcon>( "icon" );

			icon.Item = item;
			icon.Style.BackgroundColor = item.IconColor;
			icon.Style.Dirty();

			Icons.Add( icon );
		}

		/*
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
		*/

		public override void Tick()
		{
			SetClass( "hidden", !Hud.IsLocalPlaying() );

			if ( NextIconUpdate )
			{
				var iconCount = Icons.Count;

				for ( var i = 0; i < iconCount; i++ )
				{
					var icon = Icons[i];
					icon.Update();
				}

				NextIconUpdate = iconCount / 200f;
			}
		}

		private void OnFogActiveChanged( bool isActive )
		{
			if ( isActive )
			{
				var textureBuilder = RTS.Fog.TextureBuilder;

				if ( textureBuilder != null )
				{
					Fog.Style.SetBackgroundImage( textureBuilder.Texture );
				}
			}
			else
			{
				Fog.Style.Background = default;
				Fog.Style.Dirty();
			}
		}
	}
}
