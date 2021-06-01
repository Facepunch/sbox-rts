using System;
using System.Threading.Tasks;
using Sandbox;

namespace Gamelib.Network
{
	public static class PropertyWatcher
	{
		public static async void Watch<T>( Func<T> getter, Action<T,T> callback )
		{
			var currentValue = getter();

			while ( true )
			{
				await Task.Delay( 100 );

				var newValue = getter();

				if ( !currentValue.Equals( newValue ) )
				{
					var oldValue = currentValue;
					currentValue = newValue;
					callback( currentValue, oldValue );
				}
			}
		}
	}
}
