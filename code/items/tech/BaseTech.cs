namespace RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override Color Color => Color.Green;

		public override bool CanHave( Player player )
		{
			return !Has( player ) && HasDependencies( player );
		}
	}
}
