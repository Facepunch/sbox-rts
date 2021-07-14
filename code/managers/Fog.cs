using Gamelib.FlowFields.Entities;
using Gamelib.FlowFields.Grid;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

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

		private static IEnumerable<SceneParticleObject> _particleContainers;
		private static Texture _texture;
		private static float _pixelScale;
		private static int _halfResolution;
		private static int _resolution;
		private static IntVector3 _origin;
		private static byte[] _data;

		public static void Initialize()
		{
			Host.AssertClient();

			Renderer = new FogRenderer
			{
				Texture = _texture,
				Position = Vector3.Zero
			};

			if ( FlowFieldGround.Exists )
				Bounds.SetFrom( FlowFieldGround.Bounds );
			else
				Bounds.SetSize( 30000f );

			_origin = IntVector3.From( Bounds.Origin );

			Renderer.RenderBounds = new BBox( Bounds.TopLeft, Bounds.BottomRight );

			UpdateTextureSize();

			FlowFieldGround.OnUpdated += OnGroundUpdated;

			Clear();
		}

		public static void MakeVisible( Player player, Vector3 position, int range )
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
					FillRegion( IntVector3.From( data.LastPosition ), data.Object.LineOfSight, 200 );
					_viewers.RemoveAt( i );
					break;
				}
			}
		}

		public static bool IsAreaSeen( Vector3 location )
		{
			var pixelScale = (_resolution / Bounds.Size);
			var origin = location - Bounds.Origin;
			var x = (origin.x * pixelScale).CeilToInt() + (_resolution / 2);
			var y = (origin.y * pixelScale).CeilToInt() + (_resolution / 2);
			var i = ((y * _resolution) + x);

			if ( i <= 0 || i > _resolution * _resolution )
				return false;

			return (_data[i] <= 200);
		}

		[ClientRpc]
		public static void Clear()
		{
			for ( int x = 0; x < _resolution; x++ )
			{
				for ( int y = 0; y < _resolution; y++ )
				{
					var index = ((x * _resolution) + y);
					_data[index + 0] = 255;
				}
			}
		}

		[ClientRpc]
		public static void MakeVisible( Vector3 position, int range )
		{
			var vector = IntVector3.From( position );

			PunchHole( vector, range );
			FillRegion( vector, range, 200 );
		}

		internal static float SdCircle( IntVector3 p, int r )
		{
			return p.Length() - r;
		}

		public static void PaintDot( IntVector3 p, int r, int index, float smooth )
		{
			byte sdf = (byte)Math.Clamp( SdCircle( p, r ) * smooth * 255, 0, 255 );
			_data[ index ] = Math.Min( sdf, _data[ index ] );
		}

		public struct IntVector2
		{
			public int x;
			public int y;

			public IntVector2( int x, int y )
			{
				this.x = x;
				this.y = y;
			}
		}

		private static Dictionary<int, int> _squareRootCache = new();

		public struct IntVector3
		{
			public static IntVector3 From( Vector3 position )
			{
				return new IntVector3()
				{
					x = (int)position.x,
					y = (int)position.y,
					z = (int)position.z
				};
			}

			public IntVector3( int x, int y, int z )
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}

			public int x;
			public int y;
			public int z;

			public int Length()
			{
				var squared = x * x + y * y;

				if ( _squareRootCache.TryGetValue( squared, out var length ) )
					return length;

				length = (int)Math.Sqrt( x * x + y * y );

				_squareRootCache.Add( squared, length );

				return length;
			}

			public static IntVector3 operator -( IntVector3 a, IntVector3 b ) => new( a.x - b.x, a.y - b.y, a.z - b.z );
			public static IntVector3 operator -( IntVector3 a, IntVector2 b ) => new( a.x - b.x, a.y - b.y, a.z );
			public static IntVector3 operator +( IntVector3 a, int b ) => new( a.x + b, a.y + b, a.z + b );
			public static IntVector3 operator *( IntVector3 a, float b ) => new( (int)(a.x * b), (int)(a.y * b), (int)(a.z * b) );
			public static IntVector3 operator *( IntVector3 a, int b ) => new( a.x * b, a.y * b, a.z * b );
		}

		public static void PunchHole( IntVector3 location, int range )
		{
			var origin = location - _origin;
			var radius = (int)(range * _pixelScale);
			var center = (origin * _pixelScale) + _halfResolution;
			int renderRadius = (int)(radius * (MathF.PI * 0.5f));
			
			for ( int x = Math.Max( center.x - renderRadius, 0 ); x < Math.Min( center.x + renderRadius, _resolution - 1 ); x++ )
			{
				for ( int y = Math.Max( center.y - renderRadius, 0 ); y < Math.Min( center.y + renderRadius, _resolution - 1 ); y++ )
				{
					var index = ((y * _resolution) + x);
					PaintDot( center - new IntVector2( x, y ), radius, index, 0.25f );
				}	
			}
		}

		public static void FillRegion( IntVector3 location, int range, byte alpha )
		{
			var origin = location - _origin;
			var radius = (int)(range * _pixelScale);
			var center = (origin * _pixelScale) + _halfResolution;
			int renderRadius = (int)(radius * (MathF.PI * 0.5f));

			for ( int x = Math.Max( center.x - renderRadius, 0 ); x < Math.Min( center.x + renderRadius, _resolution - 1 ); x++ )
			{
				for( int y = Math.Max( center.y - renderRadius, 0 ); y < Math.Min( center.y + renderRadius, _resolution - 1 ); y++ )
				{
					var index = ((y * _resolution) + x);
					_data[ index ] = Math.Max( alpha, _data[ index ] );
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
			if ( _texture != null )
			{
				_texture.Dispose();
				_texture = null;
			}

			_resolution = Math.Max( ((float)(Bounds.Size / 30f)).CeilToInt(), 128 );
			_halfResolution = _resolution / 2;
			_pixelScale = _resolution / Bounds.Size;
			_texture = Texture.Create( _resolution, _resolution, ImageFormat.A8 ).Finish();
			_data = new byte[_resolution * _resolution];

			if ( Renderer == null )
			{
				Log.Error( "[Fog::UpdateTextureSize] Unable to locate Fog entity!" );
				return;
			}

			Renderer.FogMaterial.OverrideTexture( "Color", _texture );
		}

		private static void AddRange( Vector3 position, int range )
		{
			FogCullable cullable;

			PunchHole( IntVector3.From( position ), range );

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

			CheckParticleVisibility( position, renderRange );
		}

		private static void CheckParticleVisibility( Vector3 position, float range )
		{
			/*
			foreach ( var container in _particleContainers )
			{
				if ( container.ParticleRenderingEnabled )
					continue;

				if ( container.Transform.Position.Distance( position ) <= range )
				{
					container.ParticleRenderingEnabled = true;
				}
			}
			*/
		}

		private static void CullParticles()
		{
			/*
			foreach ( var container in _particleContainers )
			{
				container.ParticleRenderingEnabled = false;
			}
			*/
		}

		[Event.Tick]
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

			//_particleContainers = SceneObject.All.OfType<SceneParticleObject>();

			CullParticles();

			// Our first pass will create the seen history map.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				FillRegion( IntVector3.From( viewer.LastPosition ), viewer.Object.LineOfSight, 200 );
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

			_texture.Update( _data );
		}
	}
}
