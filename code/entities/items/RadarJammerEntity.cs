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
		public static float EffectiveRange => 1500f;

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
			Range.Size = EffectiveRange;
		}

		private void DeleteRangeEntity()
		{
			if ( Range.IsValid() )
			{
				Range.Delete();
				Range = null;
			}
		}
	}
}
