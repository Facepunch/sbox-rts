using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class ElectricResistance : BaseResistance
	{
		public override string Name => "Electric Resistance";
		public override string UniqueId => "resistance.electric";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/resistances/electric.png" );
		public override string DamageType => "shock";
	}
}
