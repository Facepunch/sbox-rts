using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;
using Gamelib.Elo;

namespace RTS
{
	public partial class Player : Entity
	{
		[Net] public bool IsSpectator { get; private set;  }
		[Net] public EloScore Elo { get; private set; }

		public Player()
		{
			Elo = new();
			Camera = new RTSCamera();
			Transmit = TransmitType.Always;
		}

		public void MakeSpectator( bool isSpectator )
		{
			IsSpectator = isSpectator;
		}
		
		public override void Simulate( Client client )
		{
			Game.Instance.Round?.UpdatePlayerPosition( this );

			base.Simulate( client );
		}
	}
}
