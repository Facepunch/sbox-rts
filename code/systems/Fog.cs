using Gamelib.Extensions;
using Gamelib.FlowFields.Entities;
using Gamelib.FlowFields.Grid;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vector3n = System.Numerics.Vector3;

namespace Facepunch.RTS
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
			public bool WasVisible;
			public bool IsVisible;

			public bool IsInRange( Vector3n position, float radius )
			{
				var targetBounds = (Object.CollisionBounds) + Object.Position;

				if ( targetBounds.Overlaps( position, radius ) )
					return true;

				return false;
			}
		}

		internal class TimedViewer : IFogViewer
		{
			public Vector3 Position { get; set; }
			public float LineOfSightRadius { get; set; }
		}

		public class FogTextureBuilder
		{
			public Texture Texture;
			public float PixelScale;
			public int HalfResolution;
			public int Resolution;
			public Vector3n Origin;
			public byte[] Data;

			public void Clear()
			{
				for ( int x = 0; x < Resolution; x++ )
				{
					for ( int y = 0; y < Resolution; y++ )
					{
						var index = ((x * Resolution) + y);
						Data[index + 0] = _unseenAlpha;
					}
				}
			}

			public bool IsAreaSeen( Vector3n location )
			{
				var origin = location - Origin;
				var x = (int)(origin.X * PixelScale) + HalfResolution;
				var y = (int)(origin.Y * PixelScale) + HalfResolution;
				var i = ((y * Resolution) + x);

				if ( i <= 0 || i > Resolution * Resolution )
					return false;

				return (Data[i] <= _seenAlpha);
			}

			public void PunchHole( Vector3n location, float range )
			{
				var origin = location - Origin;
				var radius = (int)(range * PixelScale);
				var resolution = Resolution;
				var centerPixelX = (origin.X * PixelScale) + HalfResolution;
				var centerPixelY = (origin.Y * PixelScale) + HalfResolution;
				var renderRadius = radius * ((float)Math.PI * 0.5f);
				var xMin = (int)Math.Max( centerPixelX - renderRadius, 0 );
				var xMax = (int)Math.Min( centerPixelX + renderRadius, resolution - 1 );
				var yMin = (int)Math.Max( centerPixelY - renderRadius, 0 );
				var yMax = (int)Math.Min( centerPixelY + renderRadius, resolution - 1 );

				for ( int x = xMin; x < xMax; x++ )
				{
					for ( int y = yMin; y < yMax; y++ )
					{
						var index = ((y * resolution) + x);
						var p = new Vector3n( centerPixelX - x, centerPixelY - y, 0f );
						var a = (p.Length() - radius) * 0.25f * 255;
						var b = a < 0 ? 0 : (a > 255 ? 255 : a);
						var sdf = (byte)b;
						var current = Data[index];
						Data[index] = sdf > current ? current : sdf;
					}
				}
			}

			public void FillRegion( Vector3n location, float range, byte alpha )
			{
				var origin = location - Origin;
				var resolution = Resolution;
				var radius = (int)(range * PixelScale);
				var centerPixelX = (origin.X * PixelScale) + HalfResolution;
				var centerPixelY = (origin.Y * PixelScale) + HalfResolution;
				var renderRadius = radius * ((float)Math.PI * 0.5f);
				var xMin = (int)Math.Max( centerPixelX - renderRadius, 0 );
				var xMax = (int)Math.Min( centerPixelX + renderRadius, resolution - 1 );
				var yMin = (int)Math.Max( centerPixelY - renderRadius, 0 );
				var yMax = (int)Math.Min( centerPixelY + renderRadius, resolution - 1 );

				for ( int x = xMin; x < xMax; x++ )
				{
					for ( int y = yMin; y < yMax; y++ )
					{
						var index = ((y * resolution) + x);
						Data[index] = Math.Max( alpha, Data[index] );
					}
				}
			}

			public void Update()
			{
				Texture.Update( Data );
			}

			public void Destroy()
			{
				Texture.Dispose();
			}

			public void Apply( FogRenderer renderer )
			{
				renderer.FogMaterial.OverrideTexture( "Color", Texture );
			}
		}

		public class FogBounds
		{
			public Vector3 TopLeft;
			public Vector3 TopRight;
			public Vector3 BottomRight;
			public Vector3 BottomLeft;
			public Vector3n Origin;
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
				var squareSize = Math.Max( bounds.Size.x, bounds.Size.y );
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

		public static event Action<bool> OnActiveChanged;

		public static readonly FogBounds Bounds = new();
		public static FogRenderer Renderer { get; private set; }
		public static FogTextureBuilder TextureBuilder => _textureBuilder;
		public static bool IsActive { get; private set; }

		private static readonly List<FogCullable> _cullables = new();
		private static readonly List<FogViewer> _viewers = new();

		private static byte _unseenAlpha = 240;
		private static byte _seenAlpha = 200;
		private static FogTextureBuilder _textureBuilder;

		public static void Initialize( BBox size, byte seenAlpha = 200, byte unseenAlpha = 240 )
		{
			Host.AssertClient();

			Renderer = new FogRenderer
			{
				Position = Vector3.Zero,
				RenderBounds = size * 2f
			};

			Bounds.SetFrom( size );

			_unseenAlpha = unseenAlpha;
			_seenAlpha = seenAlpha;

			UpdateTextureSize();
			Clear();
			Update();
		}

		public static void UpdateSize( BBox size )
		{
			Bounds.SetFrom( size );
			UpdateTextureSize();
		}

		public static void MakeVisible( Player player, Vector3 position, float radius )
		{
			MakeVisible( To.Multiple( player.GetAllTeamClients() ), position, radius );
		}

		[ClientRpc]
		public static void AddTimedViewer( Vector3 position, float radius, float duration )
		{
			var viewer = new TimedViewer()
			{
				Position = position,
				LineOfSightRadius = radius
			};

			AddViewer( viewer );

			_ = RemoveViewerAfter( viewer, duration );
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
			OnActiveChanged?.Invoke( true );
		}

		[ClientRpc]
		public static void Hide()
		{
			IsActive = false;
			OnActiveChanged?.Invoke( false );
		}

		public static void AddCullable( IFogCullable cullable )
		{
			_cullables.Add( new FogCullable()
			{
				IsVisible = false,
				Object = cullable
			} );

			cullable.OnVisibilityChanged( false );
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
					_textureBuilder.FillRegion( data.LastPosition, data.Object.LineOfSightRadius, _seenAlpha );
					_viewers.RemoveAt( i );
					break;
				}
			}
		}

		public static bool IsAreaSeen( Vector3n location )
		{
			return _textureBuilder.IsAreaSeen( location );
		}

		[ClientRpc]
		public static void Clear()
		{
			_textureBuilder.Clear();
		}

		[ClientRpc]
		public static void MakeVisible( Vector3n position, float range )
		{
			_textureBuilder.PunchHole( position, range );
			_textureBuilder.FillRegion( position, range, _seenAlpha );

			FogCullable cullable;

			// We multiply by 12.5% to cater for the render range.
			var renderRange = range * 1.125f;

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];

				if ( cullable.IsVisible ) continue;

				if ( cullable.IsInRange( position, renderRange ) )
				{
					cullable.Object.HasBeenSeen = true;
				}
			}
		}

		private static void UpdateTextureSize()
		{
			if ( _textureBuilder != null )
			{
				_textureBuilder.Destroy();
				_textureBuilder = null;
			}

			_textureBuilder = new FogTextureBuilder
			{
				Resolution = Math.Max( ((float)(Bounds.Size / 30f)).CeilToInt(), 128 )
			};

			_textureBuilder.HalfResolution = _textureBuilder.Resolution / 2;
			_textureBuilder.PixelScale = (_textureBuilder.Resolution / Bounds.Size);
			_textureBuilder.Texture = Texture.Create( _textureBuilder.Resolution, _textureBuilder.Resolution, ImageFormat.A8 ).Finish();
			_textureBuilder.Origin = Bounds.Origin;
			_textureBuilder.Data = new byte[_textureBuilder.Resolution * _textureBuilder.Resolution];

			if ( Renderer == null )
			{
				Log.Error( "[Fog::UpdateTextureSize] Unable to locate Fog entity!" );
				return;
			}

			_textureBuilder.Apply( Renderer );
		}

		private static void AddRange( Vector3n position, float range )
		{
			FogCullable cullable;

			_textureBuilder.PunchHole( position, range );

			// We multiply by 12.5% to cater for the render range.
			var renderRange = range * 1.125f;

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];

				if ( cullable.IsVisible ) continue;

				if ( cullable.IsInRange( position, renderRange ) )
				{
					var wasVisible = cullable.WasVisible;

					cullable.Object.HasBeenSeen = true;
					cullable.Object.MakeVisible( true );
					cullable.Object.IsVisible = true;
					cullable.IsVisible = true;

					if ( !wasVisible )
						cullable.Object.OnVisibilityChanged( true );
				}
			}

			CheckParticleVisibility( position, renderRange );
		}

		private static void CheckParticleVisibility( Vector3n position, float range )
		{
			var sceneObjects = SceneWorld.Current.SceneObjects;

			for ( int i = 0; i < sceneObjects.Count; i++ )
			{
				if ( sceneObjects[i] is not SceneParticleObject container )
					continue;

				if ( container.RenderParticles )
					continue;

				if ( container.Transform.Position.Distance( position ) <= range )
				{
					container.RenderParticles = true;
				}
			}
		}

		private static void CullParticles()
		{
			var sceneObjects = SceneWorld.Current.SceneObjects;

			for ( int i = 0; i < sceneObjects.Count; i++ )
			{
				if ( sceneObjects[i] is not SceneParticleObject container )
					continue;

				container.RenderParticles = false;
			}
		}

		private static async Task RemoveViewerAfter( IFogViewer viewer, float duration )
		{
			await GameTask.DelaySeconds( duration );
			Fog.RemoveViewer( viewer );
		}

		public static void Update()
		{
			if ( !IsActive ) return;

			FogCullable cullable;

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];
				cullable.Object.MakeVisible( false );
				cullable.Object.IsVisible = false;
				cullable.WasVisible = cullable.IsVisible;
				cullable.IsVisible = false;
			}

			CullParticles();

			// Our first pass will create the seen history map.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				_textureBuilder.FillRegion( viewer.LastPosition, viewer.Object.LineOfSightRadius, _seenAlpha );
			}

			// Our second pass will show what is currently visible.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				var position = viewer.Object.Position;
				var range = viewer.Object.LineOfSightRadius;

				AddRange( position, range );

				viewer.LastPosition = position;
			}

			_textureBuilder.Update();
		}
	}
}
