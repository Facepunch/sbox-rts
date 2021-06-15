using Sandbox;
using System;

namespace RTS
{
	[Library( "resource_metal" )]
	[Hammer.EditorModel( "models/metal/metal_large_00.vmdl" )]
	[Hammer.Model]
	public class MetalResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Metal;

		public override void Spawn()
		{
			if ( string.IsNullOrEmpty( GetModelName() ) )
				SetModel( "models/metal/metal_large_00.vmdl" );

			base.Spawn();
		}
	}
}
