using RTS.Buildings;
using Sandbox;
using Steamworks.Data;

namespace RTS
{
	public partial class BuildingEntity : ModelEntity, ISelectableEntity
	{
		public virtual bool CanMultiSelect => false;

		[Net] public string BuildableId { get; set; }
		[Net] public bool IsSelected { get; set; }
		[Net] public Player Player { get; set; }

		public BuildingEntity()
		{
			Transmit = TransmitType.Always;
		}

		public BaseBuilding GetBuilding()
		{
			return Game.Instance.FindBuildable<BaseBuilding>( BuildableId );
		}

		public void SetBuilding( BaseBuilding building )
		{
			BuildableId = building.UniqueId;

			if ( !string.IsNullOrEmpty( building.Model ) )
			{
				SetModel( building.Model );
				SetupPhysicsFromModel( PhysicsMotionType.Static );
			}
		}

		public void Select()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Add( this );
				IsSelected = true;
			}
		}

		public void Deselect()
		{
			if ( Player.IsValid() )
			{
				Player.Selection.Remove( this );
				IsSelected = false;
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

		[Event.Tick.Client]
		private void Tick()
		{
			if ( IsSelected && Player.IsValid() && Player.IsLocalPawn )
			{
				DebugOverlay.Box( this, new Color( 0f, 0.8f, 0f, 1f ) );
			}
		}
	}
}

