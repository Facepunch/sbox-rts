namespace Facepunch.RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override Color Color => Color.Yellow;

		public override bool CanHave( Player player )
		{
			return !Has( player ) && HasDependencies( player );
		}
	}
}
