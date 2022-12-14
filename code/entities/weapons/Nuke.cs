using Facepunch.RTS;
using Gamelib.Maths;
using Sandbox;
using System;

namespace Facepunch.RTS
{
	[Library]
	partial class Nuke : Projectile, IFogViewer
	{
		public float LineOfSightRadius => 250f;

		public override void ClientSpawn()
		{
			Fog.AddViewer( this );

			base.ClientSpawn();
		}

		protected override void OnDestroy()
		{
			if ( Game.IsClient )
			{
				Fog.RemoveViewer( this );
			}

			base.OnDestroy();
		}
	}
}
