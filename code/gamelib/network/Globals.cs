using Sandbox;
using System.Collections.Generic;

namespace Gamelib.Network
{
	public struct Globals<T> where T : Globals, new()
	{
		public string Name;
		public T Entity;

		public T Value
		{
			get
			{
				if ( Entity.IsValid() ) return Entity;
				Entity = Globals.Find<T>( Name );
				return Entity;
			}
		}
	}

	public partial class Globals : Entity
	{
		public static Globals<T> Define<T>( string name ) where T : Globals, new()
		{
			var handle = new Globals<T>()
			{
				Name = name
			};

			if ( Host.IsServer && !_cache.ContainsKey( name ) )
			{
				var entity = new T()
				{
					Name = name
				};

				handle.Entity = entity;
				_cache.Add( name, entity );
			}

			return handle;
		}

		public static T Find<T>( string name ) where T : Globals
		{
			if ( _cache.TryGetValue( name, out var entity ) )
			{
				return (entity as T);
			}

			return null;
		}

		private static Dictionary<string, Globals> _cache = new();

		[Net] public string Name { get; set; }

		public Globals()
		{
			Transmit = TransmitType.Always;
		}

		public override void ClientSpawn()
		{
			if ( !_cache.ContainsKey( Name ) )
				_cache.Add( Name, this );

			base.Spawn();
		}
	}
}
