using System.IO;

namespace Facepunch.RTS
{
	public class DissolvingData : StatusData
	{
		public float Damage { get; set; } = 1f;
		public float Interval { get; set; } = 0.3f;

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( Damage );
			writer.Write( Interval );

			base.Serialize( writer );
		}

		public override void Deserialize( BinaryReader reader )
		{
			Damage = reader.ReadSingle();
			Interval = reader.ReadSingle();

			base.Deserialize( reader );
		}
	}
}
