namespace Facepunch.RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override Color Color => Color.Yellow;

		public override bool CanHave( Player player )
		{
			return !IsResearching( player ) && !Has( player ) && HasDependencies( player );
		}

		public override void OnQueued( Player player )
		{
			player.Researching.Add( NetworkId );

			base.OnQueued( player );
		}

		public override void OnUnqueued( Player player )
		{
			player.Researching.Remove( NetworkId );

			base.OnUnqueued( player );
		}

		public override void OnCreated( Player player )
		{
			Audio.Play( player, "announcer.technology_researched" );
			RTS.Game.Toast( player, "Technology Researched", this );

			player.Researching.Remove( NetworkId );

			base.OnCreated( player );
		}

		public bool IsResearching( Player player )
		{
			return player.Researching.Contains( NetworkId );
		}
	}
}
