using Sandbox;
using System.Collections.Generic;

namespace Gamelib.Network
{
	public struct Globals<T> where T : Globals, new()
	{
		public string GlobalName;
		public T Entity;

		public T Value
		{
			get
			{
				if ( Entity.IsValid() ) return Entity;
				Entity = Globals.Find<T>( GlobalName );
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
				GlobalName = name
			};

			if ( Host.IsServer && !Cache.ContainsKey( name ) )
			{
				var entity = new T()
				{
					GlobalName = name
				};

				handle.Entity = entity;
				Cache.Add( name, entity );
			}

			return handle;
		}

		public static T Find<T>( string name ) where T : Globals
		{
			if ( Cache.TryGetValue( name, out var entity ) )
			{
				return (entity as T);
			}

			return null;
		}

		private static Dictionary<string, Globals> Cache = new();

		[Net] public string GlobalName { get; set; }

		public Globals()
		{
			Transmit = TransmitType.Always;
		}

		public override void ClientSpawn()
		{
			if ( !Cache.ContainsKey( GlobalName ) )
				Cache.Add( GlobalName, this );

			base.Spawn();
		}
	}
}
