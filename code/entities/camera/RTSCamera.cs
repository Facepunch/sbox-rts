using Sandbox;
using System;

namespace Facepunch.RTS
{
	public partial class RTSCamera : CameraMode
	{
		public float ZoomLevel { get; set; }
		public Vector3 LookAt { get; set; }

		public override void Activated()
		{
			var cameraConfig = Config.Current.Camera;

			FieldOfView = cameraConfig.FOV;
			ZoomLevel = 1f;
			ZNear = cameraConfig.ZNear;
			ZFar = cameraConfig.ZFar;
			Ortho = cameraConfig.Ortho;

			base.Activated();
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player )
				return;

			var cameraConfig = Config.Current.Camera;

			ZoomLevel += Input.MouseWheel * Time.Delta * 2f;
			ZoomLevel = ZoomLevel.Clamp( 0f, 1f );

			if ( cameraConfig.Ortho )
			{
				OrthoSize = 1f + ( (1f - ZoomLevel) * cameraConfig.ZoomScale );
				ZNear = 1f;
				Ortho = true;
			}
			else
			{
				FieldOfView = cameraConfig.FOV;
				ZNear = cameraConfig.ZNear;
				ZFar = cameraConfig.ZFar;
				Ortho = false;
			}

			var velocity = Vector3.Zero;
			var panSpeed = cameraConfig.PanSpeed - (cameraConfig.PanSpeed * ZoomLevel * 0.6f);

			if ( Input.Down( InputButton.Forward ) )
				velocity += Rotation.Forward.WithZ( 0f ) * panSpeed;

			if ( Input.Down( InputButton.Back ) )
				velocity += Rotation.Backward.WithZ( 0f ) * panSpeed;

			if ( Input.Down( InputButton.Left ) )
				velocity += Rotation.Left * panSpeed;

			if ( Input.Down( InputButton.Right ) )
				velocity += Rotation.Right * panSpeed;

			var lookAtPosition = (LookAt + velocity * Time.Delta);
			var worldSize = Gamemode.Instance.WorldSize.Size.x;

			lookAtPosition.x = lookAtPosition.x.Clamp( -worldSize, worldSize );
			lookAtPosition.y = lookAtPosition.y.Clamp( -worldSize, worldSize );

			LookAt = lookAtPosition;

			Vector3 eyePos;

			if ( cameraConfig.Ortho )
			{
				eyePos = LookAt + Vector3.Backward * cameraConfig.Backward;
				eyePos += Vector3.Left * cameraConfig.Left;
				eyePos += Vector3.Up * cameraConfig.Up;
			}
			else
			{
				eyePos = LookAt + Vector3.Backward * (cameraConfig.Backward - (cameraConfig.Backward * ZoomLevel * cameraConfig.ZoomScale));
				eyePos += Vector3.Left * (cameraConfig.Left - (cameraConfig.Left * ZoomLevel * cameraConfig.ZoomScale));
				eyePos += Vector3.Up * (cameraConfig.Up - (cameraConfig.Up * ZoomLevel * cameraConfig.ZoomScale));
			}

			Position = Position.LerpTo( eyePos, Time.Delta * 4f );
			var difference = LookAt - eyePos;
			Rotation = Rotation.Slerp( Rotation, Rotation.LookAt( difference, Vector3.Up ), Time.Delta * 8f );

			Sound.Listener = new Transform()
			{
				Position = lookAtPosition,
				Rotation = Rotation
			};

			Viewer = null;
		}
	}
}
