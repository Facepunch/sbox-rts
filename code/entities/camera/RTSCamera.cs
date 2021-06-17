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
				Pos = player.Position;
				Rot = player.Rotation;
			}

			FieldOfView = 60f;

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is Player player )
			{
				Pos = Pos.LerpTo( player.Position, Time.Delta * 4f );
				Rot = player.Rotation;
			}

			Viewer = null;
		}
	}
}
