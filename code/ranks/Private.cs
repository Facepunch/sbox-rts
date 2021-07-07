using Sandbox;

namespace Facepunch.RTS.Ranks
{
	[Library]
	public class Private : BaseRank
	{
		public override string Name => "Private";
		public override string UniqueId => "rank.private";
		public override Texture Icon => Texture.Load( "textures/rts/ranks/private.png" );
		public override int Kills => 2;
		public override int DamageModifier => 1;
	}
}
