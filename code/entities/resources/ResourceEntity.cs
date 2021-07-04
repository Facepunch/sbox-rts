using Sandbox;
using System;

namespace Facepunch.RTS
{
	public partial class ResourceEntity : ModelEntity, IFogCullable
	{
		public virtual ResourceType Resource => ResourceType.Stone;
		public virtual string Description => "";
		public virtual string Name => "";
		public virtual float GatherTime => 1f;
		public virtual int MaxCarry => 10;

		[Property, Net] public int Stock { get; set; } = 250;

		public bool IsLocalPlayers => false;
		public bool HasBeenSeen { get; set; }

		public void MakeVisible( bool isVisible )
		{
			if ( isVisible )
			{
				FogManager.RemoveCullable( this );
			}
		}
	

		public override void ClientSpawn()
		{
			FogManager.AddCullable( this );

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
				FogManager.RemoveCullable( this );
			}

			base.OnDestroy();
		}
	}
}
