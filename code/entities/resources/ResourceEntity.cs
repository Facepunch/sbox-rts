using Sandbox;
using System;

namespace RTS
{
	public partial class ResourceEntity : ModelEntity, IFogCullable
	{
		public virtual ResourceType Resource => ResourceType.Stone;
		public virtual string Description => "";
		public virtual Color Color => Color.Gray;
		public virtual string Name => "";
		public virtual float GatherTime => 1f;
		public virtual int MaxCarry => 10;

		[Property( Help = "How much of this resource there is left to take." )]
		[Net] public int Stock { get; set; } = 250;

		public bool IsLocalPlayers => false;
		public bool HasBeenSeen { get; set; }

		public void MakeVisible( bool isVisible )
		{
			if ( isVisible )
			{
				FogManager.Instance.RemoveCullable( this );
			}
		}
	

		public override void ClientSpawn()
		{
			FogManager.Instance.AddCullable( this );

			base.ClientSpawn();
		}


		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Transmit = TransmitType.Always;

			// Let's make sure there is stock.
			if ( Stock == 0 ) Stock = 250;
		}

		protected override void OnDestroy()
		{
			if ( IsClient )
			{
				FogManager.Instance.RemoveCullable( this );
			}

			base.OnDestroy();
		}
	}
}
