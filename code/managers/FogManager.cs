using Facepunch.RTS.Buildings;
using Facepunch.RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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

		public static FogManager Instance { get; private set; }

		public readonly Texture Texture;
		public readonly int Resolution;
		public readonly byte[] Data;
		public bool IsActive { get; private set; }
		public FogBounds Bounds => FogBounds.Instance;
		public Fog Fog { get; private set; }

		private readonly List<FogCullable> _cullables;
		private readonly List<FogViewer> _viewers;

		public FogManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
			Resolution = 1024;
			Data = new byte[Resolution * Resolution];

			_cullables = new List<FogCullable>();
			_viewers = new List<FogViewer>();

			if ( IsClient )
			{
				Texture = Texture.Create( Resolution, Resolution, ImageFormat.A8 ).Finish();

				Clear();

				Fog = new Fog
				{
					Texture = Texture,
					Position = Vector3.Zero
				};

				Fog.FogMaterial.OverrideTexture( "Color", Texture );
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
			for ( var i = _viewers.Count - 1; i >= 0; i-- )
			{
				if ( _viewers[i].Object == viewer )
				{
					_viewers.RemoveAt( i );
					break;
				}
			}
		}

		public bool IsAreaSeen( Vector3 location )
		{
			var pixelScale = (Resolution / FogBounds.Size);
			var origin = location - FogBounds.Origin;
			var x = (origin.x * pixelScale).CeilToInt() + (Resolution / 2);
			var y = (origin.y * pixelScale).CeilToInt() + (Resolution / 2);
			var i = ((y * Resolution) + x) * 1;

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
					var index = ((x * Resolution) + y) * 1;

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
			var pixelScale = (Resolution / FogBounds.Size);
			var origin = location - FogBounds.Origin;
			var radius = ((range * pixelScale) / 2f).CeilToInt();
			var px = (origin.x * pixelScale).CeilToInt() + (Resolution / 2);
			var py = (origin.y * pixelScale).CeilToInt() + (Resolution / 2);

			if ( px + radius < 0 || px - radius >= Texture.Width || py + radius < 0 || py - radius >= Texture.Height )
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

		private void FillLine( int y, int x1, int x2, byte alpha )
		{
			var w = Texture.Width;
			var h = Texture.Height;

			if ( x2 < x1 ) { x1 += x2; x2 = x1 - x2; x1 -= x2; }
			if ( x2 < 0 || x1 >= w || y < 0 || y >= h ) return;
			if ( x1 < 0 ) x1 = 0;
			if ( x2 >= w ) x2 = w - 1;

			for ( int x = x1; x <= x2; x++ )
			{
				var index = ((y * Resolution) + x) * 1;

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
