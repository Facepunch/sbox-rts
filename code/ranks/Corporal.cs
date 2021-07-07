using Sandbox;

namespace Facepunch.RTS.Ranks
{
	[Library]
	public class Corporal : BaseRank
	{
		public override string Name => "Corporal";
		public override string UniqueId => "rank.corporal";
		public override Texture Icon => Texture.Load( "textures/rts/ranks/corporal.png" );
		public override int Kills => 5;
		public override int DamageModifier => 2;
	}
}
