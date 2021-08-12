using Facepunch.RTS;

namespace Facepunch.RTS.Tech
{
    public abstract class BaseTech : BaseItem
	{
		public override Color Color => Color.Yellow;

		public override bool IsAvailable( Player player, ISelectable target )
		{
			return !IsResearching( player ) && !Has( player );
		}

		public override void OnQueued( Player player, ISelectable target )
		{
			player.Researching.Add( NetworkId );

			base.OnQueued( player, target );
		}

		public override void OnUnqueued( Player player, ISelectable target )
		{
			player.Researching.Remove( NetworkId );

			base.OnUnqueued( player, target );
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			Audio.Play( player, "announcer.technology_researched" );
			Hud.Toast( player, "Technology Researched", this );

			player.Researching.Remove( NetworkId );

			base.OnCreated( player, target );
		}

		public bool IsResearching( Player player )
		{
			return player.Researching.Contains( NetworkId );
		}
	}
}
