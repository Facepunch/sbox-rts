namespace Facepunch.RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override Color Color => Color.Yellow;

		public override bool CanHave( Player player )
		{
			return !Has( player ) && HasDependencies( player );
		}

		public override void OnCreated( Player player )
		{
			SoundManager.Play( player, "announcer.technology_researched" );
			RTS.Game.Toast( player, "Technology Researched", this );

			base.OnCreated( player );
		}
	}
}
