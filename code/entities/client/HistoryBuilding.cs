using Sandbox;

namespace Facepunch.RTS
{
	public partial class HistoryBuilding : AnimEntity, IFogCullable
	{
		public BuildingEntity Master { get; set; }
		public bool HasBeenSeen { get; set; }
		public bool IsFadingOut { get; set; }
		
		private RealTimeSince CreationTime { get; set; }

		public void MakeVisible( bool isVisible, bool wasVisible )
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

		public void Copy( BuildingEntity master )
		{
			SetModel( master.GetModelName() );

			//SequenceCycle = master.SequenceCycle;
			PlaybackRate = master.PlaybackRate;
			RenderColor = master.RenderColor;
			Sequence = master.Sequence;
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
