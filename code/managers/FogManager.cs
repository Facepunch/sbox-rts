using RTS.Buildings;
using RTS.Units;
using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RTS
{
	public partial class FogManager : Entity
	{
		public static FogManager Instance { get; private set; }

		public readonly List<IFogViewer> Viewers;
		public readonly Texture Texture;
		public readonly int Resolution;
		public readonly byte[] Data;
		public Fog Fog { get; private set; }

		public FogManager()
		{
			Instance = this;
			Transmit = TransmitType.Always;
			Viewers = new List<IFogViewer>();
			Resolution = 1024;
			//Data = new byte[Resolution * Resolution * 4];
			Data = new byte[Resolution * Resolution];

			if ( IsClient )
			{
				Texture = Texture.Create( Resolution, Resolution, ImageFormat.A8 )
				.Finish();

				Fog = new Fog
				{
					Texture = Texture,
					Position = Vector3.Zero
				};

				Fog.FogMaterial.OverrideTexture( "Color", Texture );

				for ( var i = 0; i < 100; i++ )
				{
					var v = new DebugViewer();
					v.Range = Rand.Float( 10f, 100f );
					v.Position = Vector3.Random * 1000f;
					AddViewer( v );
				}
			}
		}

		public void Update()
		{
			Clear( Color32.Black );

			for ( var i = 0; i < Viewers.Count; i++ )
			{
				var viewer = Viewers[i];

				if ( viewer is not UnitEntity )
				{
					viewer.Position += new Vector3( Rand.Float( -1f, 1f ) * 10f, Rand.Float( -1f, 1f ) * 10f );
				}

				AddVisibility( viewer.Position, viewer.Range );
			}

			Texture.Update( Data );
		}

		public void AddViewer( IFogViewer viewer )
		{
			Viewers.Add( viewer );
		}

		public void RemoveViewer( IFogViewer viewer )
		{
			Viewers.Remove( viewer );
		}

		public void Clear( Color32 color )
		{
			// Cache these because they're properties and it'll be slow otherwise.
			//var r = color.r;
			//var g = color.g;
			//var b = color.b;
			var a = color.a;

			for ( int x = 0; x < Resolution; x++ )
			{
				for ( int y = 0; y < Resolution; y++ )
				{
					var index = ((x * Resolution) + y) * 1;// 4;

					Data[index + 0] = 255;
					//Data[index + 0] = r;
					//Data[index + 1] = g;
					//Data[index + 2] = b;
					//Data[index + 3] = a;
				}
			}
		}

		public void AddVisibility( Vector3 location, float range )
		{
			var pixelScale = (Resolution / Fog.MapSize);
			var radius = ((range * pixelScale) / 2f).CeilToInt();
			var px = (location.x * pixelScale).CeilToInt() + (Resolution / 2);
			var py = (location.y * pixelScale).CeilToInt() + (Resolution / 2);

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

				if ( b != pb ) FillLine( b, a, c );
				if ( d != pd ) FillLine( d, a, c );
				if ( f != b ) FillLine( f, e, g );
				if ( h != d && h != f ) FillLine( h, e, g );

				pb = b;
				pd = d;

				if ( p < 0 )
					p += (x++ << 2) + 6;
				else
					p += ((x++ - y--) << 2) + 10;
			}
		}

		private void FillLine( int y, int x1, int x2 )
		{
			var w = Texture.Width;
			var h = Texture.Height;

			if ( x2 < x1 ) { x1 += x2; x2 = x1 - x2; x1 -= x2; }
			if ( x2 < 0 || x1 >= w || y < 0 || y >= h ) return;
			if ( x1 < 0 ) x1 = 0;
			if ( x2 >= w ) x2 = w - 1;

			for ( int x = x1; x <= x2; x++ )
			{
				var index = ((y * Resolution) + x) * 1;// 4;

				Data[index + 0] = 0;
				//Data[index + 1] = 0;
				//Data[index + 2] = 0;
				//Data[index + 3] = 0;
			}
		}
	}
}
