using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class BulletResistance : BaseResistance
	{
		public override string Name => "Bullet Resistance";
		public override string UniqueId => "resistance.bullet";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/resistances/bullet.png" );
		public override string DamageType => "bullet";
	}
}
