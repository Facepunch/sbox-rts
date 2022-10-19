using Sandbox;

namespace Facepunch.RTS
{
	[Library( "building_tunnel" )]
	public partial class TunnelEntity : BuildingEntity
	{
		[Net] public TunnelEntity Connection { get; set; }

		private RangeEntity Range;

		protected override void OnSelected()
		{
			if ( IsLocalPlayers && !Range.IsValid() )
			{
				CreateRangeEntity();
			}

			base.OnSelected();
		}

		protected override void OnDeselected()
		{
			DeleteRangeEntity();

			base.OnDeselected();

		}

		protected override void OnDestroy()
		{
			DeleteRangeEntity();

			base.OnDestroy();
		}

		private void CreateRangeEntity()
		{
			Range = new();
			Range.SetParent( this );
			Range.Position = Position;
			Range.Color = Color.Cyan;
			Range.Size = 15000f;
		}

		private void DeleteRangeEntity()
		{
			if ( Range.IsValid() )
			{
				Range.Delete();
				Range = null;
			}
		}

		public void ConnectTo( TunnelEntity other )
		{
			other.Connection = this;
			Connection = other;
		}

		public override Vector3? GetVacatePosition( UnitEntity unit )
		{
			if ( Connection.IsValid() )
			{
				var component = unit.GetComponent<TunnelTraveller>();

				if ( component  != null && component.FinishTravelTime )
					return Connection.GetFreePosition( unit, 1.5f );
			}

			return null;
		}

		protected override void ServerTick()
		{
			var occupants = Occupants;
			var occupantsCount = occupants.Count;

			if ( occupantsCount > 0 )
			{
				for ( var i = occupantsCount - 1; i >= 0; i-- )
				{
					var unit = occupants[i];
					var component = unit.GetComponent<TunnelTraveller>();

					if ( component?.FinishTravelTime == true )
						EvictUnit( unit );
				}
			}

			base.ServerTick();
		}

		protected override void OnEvicted( UnitEntity unit )
		{
			unit.RemoveComponent<TunnelTraveller>();

			base.OnEvicted( unit );
		}

		protected override void OnOccupied( UnitEntity unit )
		{
			var component = unit.AddComponent<TunnelTraveller>();
			component.FinishTravelTime = 2f;

			base.OnOccupied( unit );
		}
	}
}
