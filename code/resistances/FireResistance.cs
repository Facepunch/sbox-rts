using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class FireResistance : BaseResistance
	{
		public override string Name => "Fire Resistance";
		public override string UniqueId => "resistance.fire";
		public override Texture Icon => null;
		public override DamageFlags Flags => DamageFlags.Burn;
	}
}
