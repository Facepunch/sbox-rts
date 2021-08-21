using Sandbox;

namespace Facepunch.RTS.Upgrades
{
	[Library]
	public class BaseWeaponUpgrade : BaseUpgrade
	{
		public override void OnCreated( Player player, ISelectable target )
		{
			target.Tags.Add( "weapon_upgrade" );

			base.OnCreated( player, target );
		}

		public override bool IsAvailable( Player player, ISelectable target )
		{
			if ( target.Tags.Has( "weapon_upgrade" ) )
				return false;

			return base.IsAvailable( player, target );
		}
	}
}
