using System.IO;

namespace Facepunch.RTS
{
	public class StatusData
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
