using Sandbox;
using RTS.Buildings;

namespace RTS
{
	public partial class BuildingEntity : ModelEntity
	{
		[Net] public string BuildableId { get; set; }

		public BaseBuilding GetBuilding()
		{
			return Game.Instance.FindBuildable<BaseBuilding>( BuildableId );
		}

		public void SetBuilding( BaseBuilding building )
		{
			BuildableId = building.UniqueId;

			if ( !string.IsNullOrEmpty( building.Model ) )
				SetModel( building.Model );
		}

		public BuildingEntity()
		{
			Transmit = TransmitType.Always;
		}
	}
}

