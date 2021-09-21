
using Gamelib.Extensions;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Facepunch.RTS
{
	public class MiniMapImage : Image
	{
		public bool IsMouseDown { get; private set; }
		public bool IsCtrlDown { get; private set; }

		public Vector3 PixelToWorld( float x, float y )
		{
			var size = Box.Rect.Size;
			var fractionX = (x / size.x);
			var fractionY = (y / size.y);
			var worldSize = Gamemode.Instance.WorldSize.Size;
			var largestSide = MathF.Max( worldSize.x, worldSize.y );
			var positionX = (largestSide * fractionX) - (largestSide * 0.5f);
			var positionY = (largestSide * fractionY) - (largestSide * 0.5f);

			return new Vector3( -positionY, -positionX );
		}

		public Vector2 WorldToCoords( Vector3 position )
		{
			var worldSize = Gamemode.Instance.WorldSize.Size;
			var largestSide = MathF.Max( worldSize.x, worldSize.y );
			var rotation = (MathF.PI / 180f) * -90f;

			// We have to invert and rotate it by -90 degrees to fit within our coordinate system.
			position.y = -position.y;

			position = new Vector3(
				position.x * MathF.Cos( rotation ) - position.y * MathF.Sin( rotation ),
				position.x * MathF.Sin( rotation ) + position.y * MathF.Cos( rotation ),
				position.z
			);

			var offset = position + (worldSize * 0.5f);
			var coords = (offset / largestSide);

			coords.x = coords.x.Clamp( 0f, 1f );
			coords.y = coords.y.Clamp( 0f, 1f );

			return coords;
		}

		public float UnitsToPixels( float units )
		{
			var worldSize = Gamemode.Instance.WorldSize.Size;
			var largestSide = MathF.Max( worldSize.x, worldSize.y );
			var mapSize = Parent.Box.Rect.Size.x;
			var fraction = units / largestSide;

			return mapSize * fraction;
		}

		protected override void OnMouseMove( MousePanelEvent e )
		{
			if ( IsMouseDown && !IsCtrlDown && Local.Pawn is Player player )
			{
				player.LookAt( PixelToWorld( MousePosition.x, MousePosition.y ) );
			}

			base.OnMouseMove( e );
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{
			IsMouseDown = false;

			base.OnMouseUp( e );
		}

		protected override void OnMouseDown( MousePanelEvent e )
		{
			IsMouseDown = !IsCtrlDown;

			base.OnMouseDown( e );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Local.Pawn is Player player )
			{
				var worldPosition = PixelToWorld( MousePosition.x, MousePosition.y );

				if ( IsCtrlDown )
				{
					MiniMap.SendPing( worldPosition.ToCSV() );
					return;
				} 

				player.LookAt( worldPosition );
			}
		}

		[Event.BuildInput]
		private void BuildInput( InputBuilder builder )
		{
			IsCtrlDown = builder.Down( InputButton.Duck );
		}
	}

	public class MiniMapIcon : Panel
	{
		public IMapIconEntity Item { get; set; }
		public MiniMapImage Map { get; set; }

		public void SetSize( BBox bounds )
		{
			var objectSize = Math.Max( bounds.Size.x, bounds.Size.y );
			var iconSize = Map.UnitsToPixels( objectSize );

			Style.Width = Length.Pixels( iconSize );
			Style.Height = Length.Pixels( iconSize );
			Style.Dirty();
		}

		public void SetSize( int width, int height )
		{
			Style.Width = Length.Pixels( width );
			Style.Height = Length.Pixels( height );
			Style.Dirty();
		}

		public void Update()
		{
			if ( !Item.ShouldShowOnMap() )
			{
				SetClass( "hidden", true );
				return;
			}

			var coords = Map.WorldToCoords( Item.Position );

			Style.BackgroundColor = Item.IconColor;
			Style.Left = Length.Fraction( coords.x );
			Style.Top = Length.Fraction( coords.y );
			Style.Dirty();

			SetClass( "hidden", false );
		}
	}

	public class MiniMapAlert : Panel
	{
		public MiniMapImage Map { get; set; }
		public Vector3 Position { get; set; }
		public float Duration { get; set; }
		public RealTimeUntil KillTime { get; set; }

		public void Start( string className, MiniMapImage map, Vector3 position, float duration )
		{
			AddClass( className );
			KillTime = duration;
			Duration = duration;
			Position = position;
			Map = map;
		}

		public override void Tick()
		{
			var coords = Map.WorldToCoords( Position );
			var opacity = 0f;
			var halfDuration = Duration / 2f;
			var width = 0f;
			var height = 0f;

			if ( KillTime > halfDuration )
			{
				var fraction = (1f / halfDuration) * (Duration - KillTime);
				opacity = Easing.BounceOut( fraction );
				width = Easing.BounceOut( fraction ) * 40f;
				height = Easing.BounceOut( fraction ) * 40f;
			}
			else
			{
				var fraction = (1f / halfDuration) * KillTime;
				opacity = Easing.BounceIn( fraction );
				width = Easing.BounceIn( fraction ) * 40f;
				height = Easing.BounceIn( fraction ) * 40f;
			}

			Style.Opacity = opacity;
			Style.Left = Length.Fraction( coords.x );
			Style.Top = Length.Fraction( coords.y );
			Style.Width = Length.Pixels( width );
			Style.Height = Length.Pixels( height );
			Style.Dirty();

			if ( KillTime && !IsDeleting )
			{
				Delete();
			}

			base.Tick();
		}
	}

	public class MiniMapPing : Panel
	{
		public MiniMapImage Map { get; set; }
		public Vector3 Position { get; set; }
		public Color Color { get; set; }
		public float Duration { get; set; }
		public RealTimeUntil KillTime { get; set; }

		public void Start( MiniMapImage map, Vector3 position, float duration, Color color )
		{
			KillTime = duration;
			Duration = duration;
			Position = position;
			Color = color;
			Map = map;
		}

		public override void Tick()
		{
			var coords = Map.WorldToCoords( Position );
			var opacity = 0f;
			var halfDuration = Duration / 2f;
			var width = 0f;
			var height = 0f;

			if ( KillTime > halfDuration )
			{
				var fraction = (1f / halfDuration) * (Duration - KillTime);
				opacity = Easing.BounceOut( fraction );
				width = Easing.BounceOut( fraction ) * 40f;
				height = Easing.BounceOut( fraction ) * 40f;
			}
			else
			{
				var fraction = (1f / halfDuration) * KillTime;
				opacity = Easing.BounceIn( fraction );
				width = Easing.BounceIn( fraction ) * 40f;
				height = Easing.BounceIn( fraction ) * 40f;
			}

			Style.BackgroundColor = (Color * 0.75f).WithAlpha( 0.1f );
			Style.BorderColor = Color;
			Style.Opacity = opacity;
			Style.Left = Length.Fraction( coords.x );
			Style.Top = Length.Fraction( coords.y );
			Style.Width = Length.Pixels( width );
			Style.Height = Length.Pixels( height );
			Style.Dirty();

			if ( KillTime && !IsDeleting )
			{
				Delete();
			}

			base.Tick();
		}
	}

	public partial class MiniMap : Panel
	{
		public static MiniMap Instance { get; private set; }

		public readonly Panel IconContainer;
		public readonly MiniMapImage Map;
		public readonly Panel RotatedContainer;
		public readonly Panel CameraBox;
		public readonly Panel Fog;

		private List<MiniMapIcon> Icons;
		private RealTimeUntil NextIconUpdate;

		//public Texture ColorTexture;
		//public Texture DepthTexture;
		//public RealTimeUntil NextRender;

		[ServerCmd]
		public static void SendPing( string csv )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				var teamMembers = player.GetAllTeamClients();
				ReceivePing( To.Multiple( teamMembers ), csv.ToVector3(), player.TeamColor );
			}
		}

		[ClientRpc]
		public static void ReceiveAlert( Vector3 position, string className )
		{
			var screenPosition = position.ToScreen();
			var isOnScreen = screenPosition.x >= 0f && screenPosition.y >= 0f && screenPosition.x <= 1f && screenPosition.y <= 1f;

			// Don't show alerts if we can already see the thing happening.
			if ( !isOnScreen )
				Instance.Alert( position, 3f, className );
		}

		[ClientRpc]
		public static void ReceivePing( Vector3 position, Color color )
		{
			Instance.Ping( position, 3f, color );
		}

		public MiniMap()
		{
			StyleSheet.Load( "/ui/MiniMap.scss" );

			RTS.Fog.OnActiveChanged += OnFogActiveChanged;

			Map = AddChild<MiniMapImage>( "map" );
			RotatedContainer = Map.AddChild<Panel>( "container" );
			Fog = RotatedContainer.AddChild<Panel>( "fog" );
			IconContainer = RotatedContainer.AddChild<Panel>( "icons" );
			CameraBox = RotatedContainer.AddChild<Panel>( "camera" );

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

		public MiniMapIcon AddEntity( IMapIconEntity item, string className )
		{
			var icon = IconContainer.AddChild<MiniMapIcon>( "icon" );

			if ( !string.IsNullOrEmpty( className ) )
				icon.AddClass( className );

			icon.Map = Map;
			icon.Item = item;
			icon.Style.Dirty();

			Icons.Add( icon );

			return icon;
		}

		public void Alert( Vector3 position, float duration, string className )
		{
			var alert = RotatedContainer.AddChild<MiniMapAlert>( "alert" );
			alert.Start( className, Map, position, duration );
		}

		public void Ping( Vector3 position, float duration, Color color )
		{
			var ping = RotatedContainer.AddChild<MiniMapPing>( "ping" );
			ping.Start( Map, position, duration, color );
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
			var isLocalPlaying = Hud.IsLocalPlaying();
			SetClass( "hidden", !isLocalPlaying );

			if ( !isLocalPlaying ) return;

			var miniMapConfig = MiniMapEntity.Instance;

			if ( miniMapConfig != null && Map.Texture == null )
			{
				var texture = miniMapConfig.TexturePath.Replace( ".jpg", ".png" );
				Map.SetTexture( texture );
			}

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

			var worldPlane = new Plane( Vector3.Zero, Vector3.Up );
			var viewDirection = Screen.GetDirection( new Vector2( Screen.Width * 0.5f, Screen.Height * 0.5f ) );
			var viewRay = new Ray( CurrentView.Position, viewDirection );
			var viewHitPos = worldPlane.Trace( viewRay ).Value;
			var viewCoords = Map.WorldToCoords( viewHitPos );
			var boxSizeX = 0.15f;
			var boxSizeY = 0.1f;

			var selection = new Rect(
				viewCoords.x - (boxSizeX / 2f),
				viewCoords.y - (boxSizeY / 2f),
				boxSizeX,
				boxSizeY
			);

			if ( selection.left + selection.width > 1f )
				selection.width = 1f - selection.left;

			if ( selection.top + selection.height > 1f )
				selection.height = 1f - selection.top;

			CameraBox.Style.Left = Length.Fraction( selection.left.Clamp( 0f, 1f ) );
			CameraBox.Style.Top = Length.Fraction( selection.top.Clamp( 0f, 1f ) );
			CameraBox.Style.Width = Length.Fraction( selection.width.Clamp( 0f, 1f ) );
			CameraBox.Style.Height = Length.Fraction( selection.height.Clamp( 0f, 1f ) );
			CameraBox.Style.Dirty();
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
