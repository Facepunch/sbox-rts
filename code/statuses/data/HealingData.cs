using System.IO;

namespace Facepunch.RTS
{
	public class HealingData : StatusData
	{
		public float Amount { get; set; } = 1f;
		public float Interval { get; set; } = 0.1f;

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( Amount );
			writer.Write( Interval );

			base.Serialize( writer );
		}

		public override void Deserialize( BinaryReader reader )
		{
			Amount = reader.ReadSingle();
			Interval = reader.ReadSingle();

			base.Deserialize( reader );
		}
	}
}
