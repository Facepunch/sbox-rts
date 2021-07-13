using Gamelib.FlowFields.Entities;
using Gamelib.FlowFields.Grid;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS.Managers
{
	public static partial class Fog
	{
		internal class FogViewer
		{
			public IFogViewer Object;
			public Vector3 LastPosition;
		}

		internal class FogCullable
		{
			public IFogCullable Object;
			public bool IsVisible;
		}

		public class FogBounds
		{
			public Vector3 TopLeft;
			public Vector3 TopRight;
			public Vector3 BottomRight;
			public Vector3 BottomLeft;
			public Vector3 Origin;
			public float HalfSize => Size * 0.5f;
			public float Size;

			public void SetSize( float size )
			{
				var halfSize = size / 2f;

				TopLeft = new Vector3( -halfSize, -halfSize );
				TopRight = new Vector3( halfSize, -halfSize );
				BottomRight = new Vector3( halfSize, halfSize );
				BottomLeft = new Vector3( -halfSize, halfSize );
				Size = size;
			}

			public void SetFrom( BBox bounds )
			{
				var squareSize = Math.Max( bounds.Size.x, bounds.Size.y ) + 1000f;
				var halfSize = squareSize / 2f;
				var center = bounds.Center;

				TopLeft = center + new Vector3( -halfSize, -halfSize );
				TopRight = center + new Vector3( halfSize, -halfSize );
				BottomRight = center + new Vector3( halfSize, halfSize );
				BottomLeft = center + new Vector3( -halfSize, halfSize );
				Size = squareSize;
				Origin = center;
			}
		}

		public static readonly FogBounds Bounds = new();
		public static bool IsActive { get; private set; }
		public static FogRenderer Renderer { get; private set; }

		private static readonly List<FogCullable> _cullables = new();
		private static readonly List<FogViewer> _viewers = new();
		private static int Resolution;
		private static Texture Texture;
		private static byte[] Data;

		public static void Initialize()
		{
			Host.AssertClient();

			Renderer = new FogRenderer
			{
				Texture = Texture,
				Position = Vector3.Zero
			};

			if ( FlowFieldGround.Exists )
				Bounds.SetFrom( FlowFieldGround.Bounds );
			else
				Bounds.SetSize( 30000f );

			Renderer.RenderBounds = new BBox( Bounds.TopLeft, Bounds.BottomRight );

			UpdateTextureSize();

			FlowFieldGround.OnUpdated += OnGroundUpdated;

			Clear();
		}

		public static void MakeVisible( Player player, Vector3 position, float range )
		{
			MakeVisible( To.Single( player ), position, range );
		}

		public static void Clear( Player player )
		{
			Clear( To.Single( player ) );
		}

		public static void Show( Player player )
		{
			Show( To.Single( player ) );
		}

		public static void Hide( Player player )
		{
			Hide( To.Single( player ) );
		}

		[ClientRpc]
		public static void Show()
		{
			IsActive = true;
		}

		[ClientRpc]
		public static void Hide()
		{
			IsActive = false;
		}

		public static void AddCullable( IFogCullable cullable )
		{
			_cullables.Add( new FogCullable()
			{
				IsVisible = false,
				Object = cullable
			} );
		}

		public static void RemoveCullable( IFogCullable cullable )
		{
			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				if ( _cullables[i].Object == cullable )
				{
					_cullables.RemoveAt( i );
					break;
				}
			}
		}

		public static void AddViewer( IFogViewer viewer )
		{
			_viewers.Add( new FogViewer()
			{
				LastPosition = viewer.Position,
				Object = viewer
			} );
		}

		public static void RemoveViewer( IFogViewer viewer )
		{
			FogViewer data;

			for ( var i = _viewers.Count - 1; i >= 0; i-- )
			{
				data = _viewers[i];

				if ( data.Object == viewer )
				{
					FillRegion( data.LastPosition, data.Object.LineOfSight, 200 );
					_viewers.RemoveAt( i );
					break;
				}
			}
		}

		public static bool IsAreaSeen( Vector3 location )
		{
			var pixelScale = (Resolution / Bounds.Size);
			var origin = location - Bounds.Origin;
			var x = (origin.x * pixelScale).CeilToInt() + (Resolution / 2);
			var y = (origin.y * pixelScale).CeilToInt() + (Resolution / 2);
			var i = ((y * Resolution) + x);

			if ( i <= 0 || i > Resolution * Resolution )
				return false;

			return (Data[i] <= 200);
		}

		[ClientRpc]
		public static void Clear()
		{
			for ( int x = 0; x < Resolution; x++ )
			{
				for ( int y = 0; y < Resolution; y++ )
				{
					var index = ((x * Resolution) + y);
					Data[index + 0] = 255;
				}
			}
		}

		[ClientRpc]
		public static void MakeVisible( Vector3 position, float range )
		{
			PunchHole( position, range );
			FillRegion( position, range, 200 );
		}

		internal static float SdCircle( Vector2 p, float r )
		{
			return p.Length - r;
		}

		public static void PaintDot( Vector2 p, float r, int index, float smooth )
		{
			byte sdf =  Convert.ToByte( Math.Clamp( SdCircle( p, r ) * smooth * 255, 0, 255 ) );
			Data[ index ] = Math.Min( sdf, Data[ index ] );
		}

		public static void PunchHole( Vector3 location, float range )
		{
			var pixelScale = (Resolution / Bounds.Size);
			var origin = location - Bounds.Origin;
			var radius = (range * pixelScale).CeilToInt();
			var centerPixel = new Vector2( (origin * pixelScale) + (Resolution / 2) );
			var renderRadius = radius * ((float)Math.PI * 0.5f);
			
			for( int x = (int)Math.Max( centerPixel.x - renderRadius, 0 ); x < (int)Math.Min( centerPixel.x + renderRadius, Resolution - 1 ); x++ )
			{
				for( int y = (int)Math.Max( centerPixel.y - renderRadius, 0 ); y < (int)Math.Min( centerPixel.y + renderRadius, Resolution - 1 ); y++ )
				{
					var index = ((y * Resolution) + x);
					PaintDot( centerPixel - new Vector2( x, y ), radius, index, 0.25f );
				}	
			}
		}

		public static void FillRegion( Vector3 location, float range, byte alpha )
		{
			var pixelScale = (Resolution / Bounds.Size);
			var origin = location - Bounds.Origin;
			var radius = (range * pixelScale).CeilToInt();
			var centerPixel = new Vector2( (origin * pixelScale) + (Resolution / 2) );
			var renderRadius = radius * ((float)Math.PI * 0.5f);

			for( int x = (int)Math.Max( centerPixel.x - renderRadius, 0 ); x < (int)Math.Min( centerPixel.x + renderRadius, Resolution - 1 ); x++ )
			{
				for( int y = (int)Math.Max( centerPixel.y - renderRadius, 0 ); y < (int)Math.Min( centerPixel.y + renderRadius, Resolution - 1 ); y++ )
				{
					var index = ((y * Resolution) + x);
					Data[ index ] = Math.Max( alpha, Data[ index ] );
				}
			}
		}

		private static void OnGroundUpdated()
		{
			Bounds.SetFrom( FlowFieldGround.Bounds );
			UpdateTextureSize();
		}

		private static void UpdateTextureSize()
		{
			if ( Texture != null )
			{
				Texture.Dispose();
				Texture = null;
			}

			Resolution = Math.Max( ((float)(Bounds.Size / 30f)).CeilToInt(), 128 );
			Texture = Texture.Create( Resolution, Resolution, ImageFormat.A8 ).Finish();
			Data = new byte[Resolution * Resolution];

			if ( Renderer == null )
			{
				Log.Error( "[Fog::UpdateTextureSize] Unable to locate Fog entity!" );
				return;
			}

			Renderer.FogMaterial.OverrideTexture( "Color", Texture );
		}

		private static void AddRange( Vector3 position, float range )
		{
			FogCullable cullable;

			PunchHole( position, range );

			// We multiply by 12.5% to cater for the render range.
			var renderRange = range * 1.125f;

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];

				if ( cullable.IsVisible ) continue;

				if ( cullable.Object.Position.Distance( position ) <= renderRange )
				{
					cullable.Object.HasBeenSeen = true;
					cullable.Object.MakeVisible( true );
					cullable.IsVisible = true;
				}
			}
		}

		[Event.Tick.Client]
		private static void Tick()
		{
			if ( !IsActive ) return;

			FogCullable cullable;

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];
				cullable.IsVisible = false;
				cullable.Object.MakeVisible( false );
			}

			// Our first pass will create the seen history map.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				FillRegion( viewer.LastPosition, viewer.Object.LineOfSight, 200 );
			}

			// Our second pass will show what is currently visible.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				var position = viewer.Object.Position;
				var range = viewer.Object.LineOfSight;

				AddRange( position, range );

				viewer.LastPosition = position;
			}

			Texture.Update( Data );
		}
	}
}
