using System.IO;

namespace Facepunch.RTS
{
	public class FreezingData : DamageData
	{
		public float SpeedReduction { get; set; } = 0f;

		public override void Serialize( BinaryWriter writer )
		{
			writer.Write( SpeedReduction );

			base.Serialize( writer );
		}

		public override void Deserialize( BinaryReader reader )
		{
			SpeedReduction = reader.ReadSingle();

			base.Deserialize( reader );
		}
	}
}
