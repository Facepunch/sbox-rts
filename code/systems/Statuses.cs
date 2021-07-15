using Sandbox;

namespace Facepunch.RTS
{
	public static partial class Statuses
	{
		public static BaseStatus Create( string id )
		{
			return Library.Create<BaseStatus>( id );
		}

		public static void Apply( string id, Vector3 position, float radius )
		{
			Host.AssertServer();

			var entities = Physics.GetEntitiesInSphere( position, radius );

			foreach ( var entity in entities )
			{
				if ( entity is ISelectable target )
				{
					target.ApplyStatus( id );
				}
			}
		}
	}
}
