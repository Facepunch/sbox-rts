using System.IO;

namespace Facepunch.RTS
{
	public class ModifierData : StatusData
	{
		public float Modifier { get; set; } = 0.3f;

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( Modifier );

			base.Serialize( writer );
		}

		public override void Deserialize( BinaryReader reader )
		{
			Modifier = reader.ReadSingle();

			base.Deserialize( reader );
		}
	}
}
