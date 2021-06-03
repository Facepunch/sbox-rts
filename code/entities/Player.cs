using Sandbox;
using Gamelib.Elo;
using System.Collections.Generic;

namespace RTS
{
	public partial class Player : Entity
	{
		[Net, Local] public List<Entity> Selection { get; set; }
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public EloScore Elo { get; private set; }
		[Net] public Color TeamColor { get; set; }

		public Player()
		{
			Elo = new();
			Camera = new RTSCamera();
			TeamColor = Color.Random;
			Transmit = TransmitType.Always;
			Selection = new List<Entity>();
		}

		public void MakeSpectator( bool isSpectator )
		{
			IsSpectator = isSpectator;
		}

		public void LookAt( Entity other )
		{
			Position = Position.WithX( other.Position.x ).WithY( other.Position.y );
		}

		public override void Simulate( Client client )
		{
			var zoomOutDistance = 500f;
			var velocity = Vector3.Zero;
			var panSpeed = 1000f;

			if ( client.Input.Down( InputButton.Forward ) )
				velocity.x += panSpeed * Time.Delta;

			if ( client.Input.Down( InputButton.Back ) )
				velocity.x -= panSpeed * Time.Delta;

			if ( client.Input.Down( InputButton.Left ) )
				velocity.y += panSpeed * Time.Delta;

			if ( client.Input.Down( InputButton.Right ) )
				velocity.y -= panSpeed * Time.Delta;

			Position = (Position + velocity).WithZ( zoomOutDistance );
			Rotation = Rotation.LookAt( new Vector3(0.2f, 0f, -1f) );

			base.Simulate( client );
		}
	}
}
