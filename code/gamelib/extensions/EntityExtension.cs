using System;
using Sandbox;

namespace Gamelib.Extensions
{
	public static class EntityExtension
	{
		public static bool IsClientOwner( this Entity self, Client client )
		{
			return ( self.GetClientOwner() == client );
		}
	}
}
