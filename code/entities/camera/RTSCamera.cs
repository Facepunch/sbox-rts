using Sandbox;
using System;

namespace RTS
{
	public partial class RTSCamera : Camera
	{
		public override void Activated()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = player.EyePos;
				Rot = player.EyeRot;
			}

			FieldOfView = 60f;

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = Pos.LerpTo( player.EyePos, Time.Delta * 4f );
				Rot = player.EyeRot;
			}

			Viewer = null;
		}
	}
}
