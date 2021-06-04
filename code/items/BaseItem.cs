using Sandbox;
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
		public virtual List<string> Dependencies => new();
	}
}
