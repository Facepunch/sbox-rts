namespace Facepunch.RTS.Upgrades
{
    public abstract class BaseUpgrade : BaseItem
	{
		public override Color Color => Color.Green;

		public virtual string ChangeItemTo => null;

		public override bool Has( Player player, ISelectable target )
		{
			return target.HasUpgrade( this );
		}

		public override bool IsAvailable( Player player, ISelectable target )
		{
			return !target.IsInQueue( this ) && !target.HasUpgrade( this );
		}

		public override void OnCreated( Player player, ISelectable target )
		{
			//Audio.Play( player, "announcer.upgrade_complete" );
			Hud.Toast( player, "Upgrade Complete", this );

			base.OnCreated( player, target );
		}
	}
}
