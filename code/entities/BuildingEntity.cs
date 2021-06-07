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

		protected override void OnItemChanged( BaseBuilding item )
		{
			if ( !string.IsNullOrEmpty( item.Model ) )
			{
				SetModel( item.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
			}

			base.OnItemChanged( item );
		}
	}
}
