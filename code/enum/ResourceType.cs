using Sandbox;

namespace Facepunch.RTS
{
	public enum ResourceType
	{
		Stone = 0,
		Beer = 1,
		Metal = 2,
		Plasma = 3
	}

	public static class ResourceTypeMethods
	{
		public static Color GetColor( this ResourceType type )
		{
			if ( type == ResourceType.Stone )
				return Color.White;

			if ( type == ResourceType.Metal )
				return new Color( 0.2f, 0.2f, 0.2f );

			if ( type == ResourceType.Plasma )
				return Color.Magenta;

			if ( type == ResourceType.Beer )
				return Color.Yellow;

			return null;
		}

		public static ItemCreateError ToCreateError( this ResourceType type )
		{
			if ( type == ResourceType.Stone )
				return ItemCreateError.NotEnoughStone;

			if ( type == ResourceType.Metal )
				return ItemCreateError.NotEnoughMetal;

			if ( type == ResourceType.Plasma )
				return ItemCreateError.NotEnoughPlasma;

			if ( type == ResourceType.Beer )
				return ItemCreateError.NotEnoughBeer;

			return default;
		}

		public static Texture GetIcon( this ResourceType type )
		{
			if ( type == ResourceType.Stone )
				return Texture.Load( "textures/rts/icons/stone.png" );

			if ( type == ResourceType.Metal )
				return Texture.Load( "textures/rts/icons/metal.png" );

			if ( type == ResourceType.Plasma )
				return Texture.Load( "textures/rts/icons/plasma.png" );

			if ( type == ResourceType.Beer )
				return Texture.Load( "textures/rts/icons/beer.png" );

			return null;
		}
	}
}
