using System.IO;

namespace Facepunch.RTS
{
	public class BaseStatusData
	{
		public float Duration { get; set; } = 1f;

		public virtual void Serialize( BinaryWriter writer )
		{
			writer.Write( Duration );
		}

		public virtual void Deserialize( BinaryReader reader )
		{
			Duration = reader.ReadSingle();
		}
	}
}
