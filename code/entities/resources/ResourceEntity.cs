using Facepunch.RTS;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	public partial class ResourceEntity : ModelEntity, IFogCullable
	{
		public virtual ResourceType Resource => ResourceType.Stone;
		public virtual string Description => "";
		public virtual string Name => "";
		public virtual float GatherTime => 0.5f;
		public virtual string[] GatherSounds => new string[]
		{
			"minerock1",
			"minerock2",
			"minerock3",
			"minerock4"
		};
		public virtual int MaxCarry => 20;

		[Property, Net] public int Stock { get; set; } = 250;

		public bool IsLocalPlayers => false;
		public bool HasBeenSeen { get; set; }

		private RealTimeUntil _nextGatherSound;

		public void MakeVisible( bool isVisible )
		{
			if ( isVisible )
			{
				Fog.RemoveCullable( this );
			}
		}

		public void PlayGatherSound()
		{
			if ( !_nextGatherSound ) return;

			if ( GatherSounds.Length > 0 )
				PlaySound( Rand.FromArray( GatherSounds ) );

			_nextGatherSound = 0.5f;
		}

		public override void ClientSpawn()
		{
			Fog.AddCullable( this );

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
				Fog.RemoveCullable( this );
			}

			base.OnDestroy();
		}
	}
}
