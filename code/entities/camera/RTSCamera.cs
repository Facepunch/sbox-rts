using Sandbox;
using System;

namespace RTS
{
	public partial class RTSCamera : Camera
	{
		public float TargetFOV { get; set; } = 120f;
		public float MinFOV { get; private set; } = 60f;
		public float MaxFOV { get; private set; } = 90f;

		public override void Activated()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = player.Position;
				Rot = player.Rotation;
			}

			TargetFOV = MaxFOV;
			FieldOfView = MaxFOV;

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				FieldOfView = FieldOfView.LerpTo( TargetFOV, Time.Delta * 4f );
				Pos = Pos.LerpTo( player.Position, Time.Delta );
				Rot = player.Rotation;
			}

			Viewer = null;
		}

		public override void BuildInput( InputBuilder input )
		{
			TargetFOV += (input.MouseWheel * -10f);
			TargetFOV = TargetFOV.Clamp( MinFOV, MaxFOV );

			base.BuildInput( input );
		}
	}
}
