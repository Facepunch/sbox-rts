using Sandbox;
using System.IO;

namespace Facepunch.RTS
{
	public interface IStatus
	{
		public string Name { get; }
		public string Description { get; }
		public Texture Icon { get; }
		public ISelectable Target { get; }
		public RealTimeUntil EndTime { get; }
		public string UniqueId { get;}
		public BaseStatusData GetData();
		public void SetData( BaseStatusData data );
		public void Initialize( string uniqueId, ISelectable target );
		public void Serialize( BinaryWriter writer );
		public void Deserialize( BinaryReader reader );
		public void Restart();
		public void OnRestarted();
		public void OnApplied();
		public void OnRemoved();
		public void Tick();
	}
}
