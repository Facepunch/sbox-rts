using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "fog_bounds" )]
	public partial class FogBounds : ModelEntity
	{
		public static FogBounds Instance { get; private set; }
		public static Vector3 TopLeft { get; private set; } = new Vector3( -10000f, -10000f );
		public static Vector3 TopRight { get; private set; } = new Vector3( 10000f, -10000f );
		public static Vector3 BottomRight { get; private set; } = new Vector3( 10000f, 10000f );
		public static Vector3 BottomLeft { get; private set; } = new Vector3( -10000f, 10000f );
		public static Vector3 Origin { get; private set; }
		public static float HalfSize { get; private set; } = 5000f;
		public static float Size { get; private set; } = 10000f;

		public override void Spawn()
		{
			base.Spawn();

			EnableAllCollisions = false;
			Transmit = TransmitType.Always;
		}

		public override void ClientSpawn()
		{
			var bounds = CollisionBounds + Position;

			Size = Math.Max( bounds.Size.x, bounds.Size.y );
			HalfSize = Size * 0.5f;
			Origin = bounds.Center.WithZ( 0f );
			TopLeft = (bounds.Center + new Vector3( -HalfSize, -HalfSize )).WithZ( 0f );
			TopRight = (bounds.Center + new Vector3( HalfSize, -HalfSize )).WithZ( 0f );
			BottomRight = (bounds.Center + new Vector3( HalfSize, HalfSize )).WithZ( 0f );
			BottomLeft = (bounds.Center + new Vector3( -HalfSize, HalfSize )).WithZ( 0f );

			Instance = this;

			base.ClientSpawn();
		}
	}
}
