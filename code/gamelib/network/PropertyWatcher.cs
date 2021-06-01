using System;
using System.Threading.Tasks;
using Sandbox;

namespace Gamelib.Network
{
	public static class PropertyWatcher
	{
		public class Watcher
		{
			public bool IsWatching { get; private set; } = true;

			public void Stop()
			{
				IsWatching = false;
			}
		}

		public static Watcher Watch<T>( Func<T> getter, Action<T, T> callback )
		{
			var watcher = new Watcher();

			_ = StartPolling( getter, callback, watcher );

			return watcher;
		}

		private static async Task StartPolling<T>( Func<T> getter, Action<T, T> callback, Watcher watcher )
		{
			var currentValue = getter();

			while ( watcher.IsWatching )
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
