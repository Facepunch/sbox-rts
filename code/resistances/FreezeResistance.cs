using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class FreezeResistance : BaseResistance
	{
		public override string Name => "Freeze Resistance";
		public override string UniqueId => "resistance.freeze";
		public override Texture Icon => Texture.Load( "textures/rts/resistances/fire.png" );
		public override DamageFlags Flags => DamageFlags.BlastWaterSurface;
	}
}
