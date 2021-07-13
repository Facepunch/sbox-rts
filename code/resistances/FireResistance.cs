using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class FireResistance : BaseResistance
	{
		public override string Name => "Fire Resistance";
		public override string UniqueId => "resistance.fire";
		public override Texture Icon => Texture.Load( "textures/rts/resistances/fire.png" );
		public override DamageFlags Flags => DamageFlags.Burn;
	}
}
