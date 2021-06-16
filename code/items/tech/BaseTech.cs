namespace RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override bool CanHave( Player player )
		{
			return !Has( player ) && HasDependencies( player );
		}
	}
}
