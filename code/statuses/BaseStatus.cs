using Sandbox;
using System;

namespace Facepunch.RTS
{
	public abstract class BaseStatus
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual float Duration => 1f;
		public virtual Texture Icon => null;

		public ISelectable Target { get; private set; }
		public RealTimeUntil EndTime { get; private set; }
		public string UniqueId { get; private set; }

		public virtual void Initialize( string uniqueId, ISelectable target )
		{
			UniqueId = uniqueId;
			EndTime = Duration;
			Target = target;
		}

		public void Restart()
		{
			EndTime = Duration;
			OnRestarted();
		}

		public virtual void OnRestarted() { }

		public virtual void OnApplied() { }

		public virtual void OnRemoved() { }

		public virtual void Tick() { }
	}
}
