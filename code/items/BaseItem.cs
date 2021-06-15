using Sandbox;
using System;
using System.Collections.Generic;

namespace RTS
{
    public abstract class BaseItem
	{
		public uint NetworkId { get; set; }
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual string Description => "";
		public virtual int BuildTime => 0;
		public virtual Dictionary<ResourceType, int> Costs => new();
		public virtual HashSet<string> Dependencies => new();

		public bool HasDependencies( Player player )
		{
			foreach ( var v in Dependencies )
			{
				var dependency = ItemManager.Instance.Find<BaseItem>( v );

				if ( dependency == null )
					throw new Exception( "Unable to locate item by id: " + v );

				if ( !player.Dependencies.Contains( dependency.NetworkId ) )
					return false;
			}

			return true;
		}
	}
}
