using RTS.Buildings;
using Sandbox;
using Steamworks.Data;

namespace RTS
{
	public partial class BuildingEntity : ItemEntity<BaseBuilding>
	{
		public BuildingEntity() : base()
		{
			Tags.Add( "building", "selectable" );
		}

		protected override void OnPlayerAssigned( Player player )
		{
			var item = Item;

			if ( !player.Dependencies.Contains( item.NetworkId ) )
				player.Dependencies.Add( item.NetworkId );

			base.OnPlayerAssigned( player );
		}

		protected override void OnItemChanged( BaseBuilding item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
			}

			Health = item.MaxHealth;

			base.OnItemChanged( item );
		}
	}
}
