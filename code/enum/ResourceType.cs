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
				return new Color( 0.4f, 0.4f, 0.7f );

			if ( type == ResourceType.Plasma )
				return Color.Magenta;

			if ( type == ResourceType.Beer )
				return Color.Yellow;

			return null;
		}

		public static RequirementError ToRequirementError( this ResourceType type )
		{
			if ( type == ResourceType.Stone )
				return RequirementError.NotEnoughStone;

			if ( type == ResourceType.Metal )
				return RequirementError.NotEnoughMetal;

			if ( type == ResourceType.Plasma )
				return RequirementError.NotEnoughPlasma;

			if ( type == ResourceType.Beer )
				return RequirementError.NotEnoughBeer;

			return default;
		}

		public static Texture GetIcon( this ResourceType type )
		{
			if ( type == ResourceType.Stone )
				return Texture.Load( FileSystem.Mounted, "textures/rts/icons/stone.png" );

			if ( type == ResourceType.Metal )
				return Texture.Load( FileSystem.Mounted, "textures/rts/icons/metal.png" );

			if ( type == ResourceType.Plasma )
				return Texture.Load( FileSystem.Mounted, "textures/rts/icons/plasma.png" );

			if ( type == ResourceType.Beer )
				return Texture.Load( FileSystem.Mounted, "textures/rts/icons/beer.png" );

			return null;
		}
	}
}
