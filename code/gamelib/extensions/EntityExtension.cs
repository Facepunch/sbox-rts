using System;
using Sandbox;

namespace Gamelib.Extensions
{
	public static class EntityExtension
	{
		public static float GetDiameterXY( this ModelEntity self, float scalar = 1f )
		{
			var modelSize = self.CollisionBounds.Size;
			return MathF.Max( modelSize.x, modelSize.y ) * scalar;
		}

		public static bool IsClientOwner( this Entity self, Client client )
		{
			return ( self.GetClientOwner() == client );
		}
	}
}
