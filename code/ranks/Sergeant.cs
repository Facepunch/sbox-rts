using Sandbox;

namespace Facepunch.RTS
{
	[Library]
	public class Sergeant : BaseRank
	{
		public override string Name => "Sergeant";
		public override string UniqueId => "rank.sergeant";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/ranks/sergeant.png" );
		public override int Kills => 10;
		public override int DamageModifier => 3;
	}
}
