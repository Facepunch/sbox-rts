using Sandbox;
using System;

namespace RTS
{
	[Library( "resource_stone" )]
	[Hammer.EditorModel( "models/rocks/rock_large_00.vmdl" )]
	[Hammer.Model]
	public partial class StoneResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Stone;

		public override void Spawn()
		{
			if ( string.IsNullOrEmpty( GetModelName() ) )
				SetModel( "models/rocks/rock_large_00.vmdl" );

			base.Spawn();
		}
	}
}
