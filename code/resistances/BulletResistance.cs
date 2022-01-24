using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class BulletResistance : BaseResistance
	{
		public override string Name => "Bullet Resistance";
		public override string UniqueId => "resistance.bullet";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/resistances/bullet.png" );
		public override DamageFlags Flags => DamageFlags.Bullet;
	}
}
