using Facepunch.RTS;

namespace Facepunch.RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public virtual bool AlwaysShowInList => true;
		public override Color Color => Color.Yellow;

		public override bool IsAvailable( RTSPlayer player, ISelectable target )
		{
			if ( !AlwaysShowInList && !HasDependencies( player ) )
				return false;

			return !IsResearching( player ) && !Has( player );
		}

		public override void OnQueued( RTSPlayer player, ISelectable target )
		{
			player.Researching.Add( NetworkId );

			base.OnQueued( player, target );
		}

		public override void OnUnqueued( RTSPlayer player, ISelectable target )
		{
			player.Researching.Remove( NetworkId );

			base.OnUnqueued( player, target );
		}

		public override void OnCreated( RTSPlayer player, ISelectable target )
		{
			Audio.Play( player, "announcer.technology_researched" );
			Hud.Toast( player, "Technology Researched", this );

			player.Researching.Remove( NetworkId );

			base.OnCreated( player, target );
		}

		public bool IsResearching( RTSPlayer player )
		{
			return player.Researching.Contains( NetworkId );
		}
	}
}
