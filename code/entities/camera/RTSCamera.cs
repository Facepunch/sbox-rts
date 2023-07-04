using Sandbox;
using System;

namespace Facepunch.RTS
{
	public partial class RTSCamera : BaseNetworkable
	{
		public float ZoomLevel { get; set; }
		public Vector3 LookAt { get; set; }

		public void Update()
		{
			ZoomLevel += Input.MouseWheel * Time.Delta * 8f;
			ZoomLevel = ZoomLevel.Clamp( 0f, 1f );

			Camera.FieldOfView = 30f;
			Camera.ZNear = 1000f;
			Camera.ZFar = 10000f;

			var velocity = Vector3.Zero;
			var panSpeed = 5000f - (5000f * ZoomLevel * 0.6f);

			if ( Input.Down( "run" ) )
   				panSpeed *= 2f;

			if ( Input.Down( "forward" ) )
				velocity += Camera.Rotation.Forward.WithZ( 0f ) * panSpeed;

			if ( Input.Down( "backward" ) )
				velocity += Camera.Rotation.Backward.WithZ( 0f ) * panSpeed;

			if ( Input.Down( "left" ) )
				velocity += Camera.Rotation.Left * panSpeed;

			if ( Input.Down( "right" ) )
				velocity += Camera.Rotation.Right * panSpeed;

			var lookAtPosition = (LookAt + velocity * Time.Delta);
			var worldSize = RTSGame.Entity.WorldSize.Size.x;

			lookAtPosition.x = lookAtPosition.x.Clamp( -worldSize, worldSize );
			lookAtPosition.y = lookAtPosition.y.Clamp( -worldSize, worldSize );

			LookAt = lookAtPosition;

			Vector3 eyePos;

			eyePos = LookAt + Vector3.Backward * (2500f - (2500f * ZoomLevel * 0.6f));
			eyePos += Vector3.Left * (2500f - (2500f * ZoomLevel * 0.6f));
			eyePos += Vector3.Up * (5000f - (5000f * ZoomLevel * 0.6f));

			Camera.Position = Camera.Position.LerpTo( eyePos, Time.Delta * 4f );
			var difference = LookAt - eyePos;
			Camera.Rotation = Rotation.Slerp( Camera.Rotation, Rotation.LookAt( difference, Vector3.Up ), Time.Delta * 8f );

			Sound.Listener = new Transform()
			{
				Position = lookAtPosition,
				Rotation = Camera.Rotation
			};

			Camera.FirstPersonViewer = null;
		}
	}
}
