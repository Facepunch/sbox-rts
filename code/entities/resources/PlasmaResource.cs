using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library( "resource_plasma" )]
	[Hammer.Model( Model = "models/rocks/rock_large_00.vmdl", MaterialGroup = "Plasma" )]
	public class PlasmaResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Plasma;
		public override int DefaultStock => 250;
		public override string Description => "You can mine this to gather Plasma for your empire.";
		public override string Name => "Plasma";
		public Particles Effect { get; private set; }

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			// TODO: We have to spawn them on the client for some reason.
			Effect = Particles.Create( "particles/plasma/plasma_effect.vpcf" );
			Effect.SetEntity( 0, this );
		}

		protected override void OnDestroy()
		{
			if ( Effect != null )
				Effect.Destroy( false );

			base.OnDestroy();
		}
	}
}
