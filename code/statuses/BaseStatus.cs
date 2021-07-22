using Sandbox;
using System;
using System.IO;

namespace Facepunch.RTS
{
	public abstract class BaseStatus<T> : IStatus where T : StatusData, new()
	{
		public virtual string Name => "";
		public virtual string Description => "";
		public virtual Texture Icon => null;

		public ISelectable Target { get; private set; }
		public RealTimeUntil EndTime { get; private set; }
		public string UniqueId { get; private set; }
		public T Data { get; private set; }

		public virtual void Initialize( string uniqueId, ISelectable target )
		{
			Target = target;
			EndTime = Data.Duration;
			UniqueId = uniqueId;
		}

		public virtual void Serialize( BinaryWriter writer )
		{
			Data.Serialize( writer );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			if ( Data == null ) Data = new T();
			Data.Deserialize( reader );
		}

		public StatusData GetData() => Data;

		public void SetData( StatusData data )
		{
			Data = (data as T);
		}

		public void Restart()
		{
			EndTime = Data.Duration;
			OnRestarted();
		}

		public virtual void OnRestarted() { }

		public virtual void OnApplied() { }

		public virtual void OnRemoved() { }

		public virtual void Tick() { }
	}
}
