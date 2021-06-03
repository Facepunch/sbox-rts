using Sandbox;
using RTS.Buildings;

namespace RTS
{
	public partial class BuildingEntity : ModelEntity, ISelectableEntity
	{
		[Net] public string BuildableId { get; set; }
		[Net] public bool IsSelected { get; set; }
		[Net] public Player Player { get; set; }

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

		public void Select()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Add( this );
			}
		}

		public void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
			}
		}

		public void Highlight()
		{
			throw new System.NotImplementedException();
		}

		public void Unhighlight()
		{
			throw new System.NotImplementedException();
		}

		public BuildingEntity()
		{
			Transmit = TransmitType.Always;
		}
	}
}

