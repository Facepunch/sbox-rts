using Sandbox;
using System;
using System.Linq;

namespace Facepunch.RTS
{
	public static partial class Statuses
	{
		public static IStatus Create( string id )
		{
			return Library.Create<IStatus>( id );
		}

		public static void Apply<S>( Vector3 position, float radius, StatusData data, Func<ISelectable, bool> filter = null ) where S : IStatus
		{
			Host.AssertServer();

			var entities = Entity.FindInSphere( position, radius ).OfType<ISelectable>();

			if ( filter != null )
			{
				entities = entities.Where( filter );
			}

			foreach ( var entity in entities )
			{
				entity.ApplyStatus<S>( data );
			}
		}
	}
}
