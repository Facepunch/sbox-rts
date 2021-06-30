using Gamelib.FlowFields.Entities;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	public partial class FogManager : Entity
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

		public static FogManager Instance { get; private set; }

		public readonly FogBounds Bounds = new();
		public Texture Texture;
		public int Resolution;
		public byte[] Data;
		public bool IsActive { get; private set; }
		public Fog Fog { get; private set; }

		private readonly List<FogCullable> _cullables;
		private readonly List<FogViewer> _viewers;

		public FogManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;

			_cullables = new List<FogCullable>();
			_viewers = new List<FogViewer>();

			if ( IsClient )
			{
				Clear();
			}
		}

		public void MakeVisible( Player player, Vector3 position, float range )
		{
			MakeVisible( To.Single( player ), position, range );
		}

		public void Clear( Player player )
		{
			Clear( To.Single( player ) );
		}

		public void Show( Player player )
		{
			Show( To.Single( player ) );
		}

		public void Hide( Player player )
		{
			Hide( To.Single( player ) );
		}

		[ClientRpc]
		public void Show()
		{
			IsActive = true;
		}

		[ClientRpc]
		public void Hide()
		{
			IsActive = false;
		}

		public void AddCullable( IFogCullable cullable )
		{
			_cullables.Add( new FogCullable()
			{
				IsVisible = false,
				Object = cullable
			} );
		}

		public void RemoveCullable( IFogCullable cullable )
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

		public void AddViewer( IFogViewer viewer )
		{
			_viewers.Add( new FogViewer()
			{
				LastPosition = viewer.Position,
				Object = viewer
			} );
		}

		public void RemoveViewer( IFogViewer viewer )
		{
			FogViewer data;

			for ( var i = _viewers.Count - 1; i >= 0; i-- )
			{
				data = _viewers[i];

				if ( data.Object == viewer )
				{
					PunchHole( data.LastPosition, data.Object.LineOfSight, 200 );
					_viewers.RemoveAt( i );
					break;
				}
			}
		}

		public bool IsAreaSeen( Vector3 location )
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
		public void Clear()
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
		public void MakeVisible( Vector3 position, float range )
		{
			AddRange( position, range, 200 );
		}

		public void PunchHole( Vector3 location, float range, byte alpha )
		{
			var pixelScale = (Resolution / Bounds.Size);
			var origin = location - Bounds.Origin;
			var radius = ((range * pixelScale) / 2f).CeilToInt();
			var px = (origin.x * pixelScale).CeilToInt() + (Resolution / 2);
			var py = (origin.y * pixelScale).CeilToInt() + (Resolution / 2);

			if ( px + radius < 0 || px - radius >= Resolution || py + radius < 0 || py - radius >= Resolution )
				return;

			int x = 0;
			int y = radius;
			int p = 3 - (radius << 1);
			int a, b, c, d, e, f, g, h;
			int pb = py + radius + 1;
			int pd = py + radius + 1;

			while ( x <= y )
			{
				a = px + x;
				b = py + y;
				c = px - x;
				d = py - y;
				e = px + y;
				f = py + x;
				g = px - y;
				h = py - x;

				if ( b != pb ) FillLine( b, a, c, alpha );
				if ( d != pd ) FillLine( d, a, c, alpha );
				if ( f != b ) FillLine( f, e, g, alpha );
				if ( h != d && h != f ) FillLine( h, e, g, alpha );

				pb = b;
				pd = d;

				if ( p < 0 )
					p += (x++ << 2) + 6;
				else
					p += ((x++ - y--) << 2) + 10;
			}
		}

		public override void ClientSpawn()
		{
			Fog = new Fog
			{
				Texture = Texture,
				Position = Vector3.Zero
			};

			if ( FlowFieldGround.Exists )
				Bounds.SetFrom( FlowFieldGround.Bounds );
			else
				Bounds.SetSize( 30000f );

			Fog.RenderBounds = new BBox( Bounds.TopLeft, Bounds.BottomRight );

			UpdateTextureSize();

			FlowFieldGround.OnUpdated += OnGroundUpdated;

			base.ClientSpawn();
		}

		private void OnGroundUpdated()
		{
			Bounds.SetFrom( FlowFieldGround.Bounds );
			UpdateTextureSize();
		}

		private void UpdateTextureSize()
		{
			if ( Texture != null )
			{
				Texture.Dispose();
				Texture = null;
			}

			Resolution = ((float)(Bounds.Size / 20f)).CeilToInt();
			Texture = Texture.Create( Resolution, Resolution, ImageFormat.A8 ).Finish();
			Data = new byte[Resolution * Resolution];

			if ( Fog == null )
			{
				Log.Error( "[FogManager::UpdateTextureSize] Unable to locate Fog entity!" );
				return;
			}

			Fog.FogMaterial.OverrideTexture( "Color", Texture );
		}

		private void FillLine( int y, int x1, int x2, byte alpha )
		{
			var w = Resolution;
			var h = Resolution;

			if ( x2 < x1 ) { x1 += x2; x2 = x1 - x2; x1 -= x2; }
			if ( x2 < 0 || x1 >= w || y < 0 || y >= h ) return;
			if ( x1 < 0 ) x1 = 0;
			if ( x2 >= w ) x2 = w - 1;

			for ( int x = x1; x <= x2; x++ )
			{
				var index = ((y * Resolution) + x);

				Data[index + 0] = alpha;
			}
		}

		private void AddRange( Vector3 position, float range, byte alpha )
		{
			FogCullable cullable;

			PunchHole( position, range, alpha );

			for ( var i = _cullables.Count - 1; i >= 0; i-- )
			{
				cullable = _cullables[i];

				if ( cullable.IsVisible ) continue;

				if ( cullable.Object.Position.Distance( position ) <= range / 2f )
				{
					cullable.Object.HasBeenSeen = true;
					cullable.Object.MakeVisible( true );
					cullable.IsVisible = true;
				}
			}
		}

		[Event.Tick.Client]
		private void Tick()
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
				PunchHole( viewer.LastPosition, viewer.Object.LineOfSight, 200 );
			}

			// Our second pass will show what is currently visible.
			for ( var i = 0; i < _viewers.Count; i++ )
			{
				var viewer = _viewers[i];
				var position = viewer.Object.Position;
				var range = viewer.Object.LineOfSight;

				AddRange( position, range, 0 );

				viewer.LastPosition = position;
			}

			Texture.Update( Data );
		}
	}
}
