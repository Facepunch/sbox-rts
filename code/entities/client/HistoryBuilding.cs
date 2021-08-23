﻿using Sandbox;

namespace Facepunch.RTS
{
	public partial class HistoryBuilding : AnimEntity, IFogCullable
	{
		public BuildingEntity Master { get; set; }
		public bool HasBeenSeen { get; set; }
		public bool IsFadingOut { get; set; }

		public void MakeVisible( bool isVisible, bool wasVisible )
		{
			if ( isVisible )
			{
				IsFadingOut = true;
			}
		}

		public void Copy( BuildingEntity master )
		{
			SetModel( master.GetModel() );

			PlaybackRate = master.PlaybackRate;
			RenderColor = master.RenderColor;
			Sequence = master.Sequence;
			Rotation = master.Rotation;
			Position = master.Position;
			//AnimCycle = master.AnimCycle;
			//AnimTime = master.AnimTime;
			Master = master;
		}

		public override void Spawn()
		{
			Fog.AddCullable( this );

			base.Spawn();
		}

		protected override void OnDestroy()
		{
			Fog.RemoveCullable( this );

			base.OnDestroy();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( !IsFadingOut ) return;

			RenderAlpha = RenderAlpha.LerpTo( 0f, Time.Delta * 2f );

			if ( RenderAlpha <= 0f )
			{
				IsFadingOut = false;
				Delete();
			}
		}
	}
}
