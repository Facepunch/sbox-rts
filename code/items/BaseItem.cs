using Sandbox;
using System.Collections.Generic;

namespace RTS
{
    public abstract class BaseItem
	{
		public virtual string Name => "";
		public virtual string UniqueId => "";
		public virtual string Description => "";
		public virtual int BuildTime => 0;
		public virtual ResourceType Resource => ResourceType.Beer;
		public virtual int Cost => 0;
		public virtual List<string> Dependencies => new();
	}
}
