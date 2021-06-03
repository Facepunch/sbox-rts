using Sandbox;
using System;

namespace RTS
{
	public partial class RTSCamera : Camera
	{
		public float TargetFieldOfView { get; set; }

		public override void Activated()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = player.Position;
				Rot = player.Rotation;
			}

			TargetFieldOfView = 90f;
			FieldOfView = 90f;

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				FieldOfView = FieldOfView.LerpTo( TargetFieldOfView, Time.Delta * 4f );
				Pos = Pos.LerpTo( player.Position, Time.Delta );
				Rot = player.Rotation;
			}

			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{
			TargetFieldOfView += (input.MouseWheel * 10f);
			TargetFieldOfView = TargetFieldOfView.Clamp(60f, 120f );

			base.BuildInput( input );
		}
	}
}
