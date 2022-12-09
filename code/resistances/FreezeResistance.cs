using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class FreezeResistance : BaseResistance
	{
		public override string Name => "Freeze Resistance";
		public override string UniqueId => "resistance.freeze";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/resistances/fire.png" );
		public override string DamageType => "cold";
	}
}
