using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Statuses
	{
		public static IStatus Create( string id )
		{
			return Library.Create<IStatus>( id );
		}

		public static void Apply<S>( Vector3 position, float radius, StatusData data ) where S : IStatus
		{
			Host.AssertServer();

			var entities = Physics.GetEntitiesInSphere( position, radius );

			foreach ( var entity in entities )
			{
				if ( entity is ISelectable target )
				{
					target.ApplyStatus<S>( data );
				}
			}
		}
	}
}
