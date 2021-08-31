using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "building_radar_jammer" )]
	public partial class RadarJammerEntity : BuildingEntity
	{
		private RangeEntity _range;

		protected override void OnSelected()
		{
			if ( IsLocalPlayers && !_range.IsValid() )
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
			_range = new();
			_range.SetParent( this );
			_range.Position = Position;
			_range.Color = Color.Cyan;
			_range.Size = 1000f;
		}

		private void DeleteRangeEntity()
		{
			if ( _range.IsValid() )
			{
				_range.Delete();
				_range = null;
			}
		}
	}
}
