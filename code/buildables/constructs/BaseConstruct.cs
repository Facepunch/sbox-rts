using System.Collections.Generic;

namespace RTS.Constructs
{
    public abstract class BaseConstruct : BaseBuildable
	{
		public virtual List<string> Buildables => new();
		public virtual string Model => "";
	}
}
