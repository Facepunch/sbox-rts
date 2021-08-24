﻿using Sandbox;

namespace Facepunch.RTS
{
	public partial class HistoryBuilding : AnimEntity, IFogCullable
	{
		public BuildingEntity Master { get; set; }
		public bool HasBeenSeen { get; set; }
		public bool IsFadingOut { get; set; }
		
		private RealTimeSince CreationTime { get; set; }

		public void OnVisibilityChanged( bool isVisible )
		{
			if ( isVisible )
			{
				if ( Master.IsValid() )
				{
					Delete();
					return;
				}

				IsFadingOut = true;
			}
		}

		public void MakeVisible( bool isVisible ) { }

		public void Copy( BuildingEntity master )
		{
			SetModel( master.GetModelName() );

			//CurrentSequence.TimeNormalized = master.CurrentSequence.TimeNormalized;
			//CurrentSequence.Name = master.CurrentSequence.Name;

			Log.Info( "Copying: " + master.RenderAlpha );

			PlaybackRate = master.PlaybackRate;
			RenderColor = master.RenderColor;
			RenderAlpha = master.RenderAlpha;
			Rotation = master.Rotation;
			Position = master.Position;
			Master = master;
		}

		public override void Spawn()
		{
			Fog.AddCullable( this );

			CreationTime = 0f;

			base.Spawn();
		}

		protected override void OnDestroy()
		{
			Fog.RemoveCullable( this );

			if ( Master.IsValid() )
			{
				Master.EnableDrawing = true;
			}

			base.OnDestroy();
		}

		[Event.Tick.Client]
		private void ClientTick()
		{
			if ( CreationTime > 0.1f && Master.IsValid() )
			{
				Master.EnableDrawing = false;
			}

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
