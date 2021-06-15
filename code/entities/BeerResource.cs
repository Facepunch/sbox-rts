using Sandbox;
using System;

namespace RTS
{
	[Library( "resource_beer" )]
	[Hammer.EditorModel( "models/barrels/square_wooden_box_gold.vmdl" )]
	[Hammer.Model]
	public partial class BeerResource : ResourceEntity
	{
		public override ResourceType Resource => ResourceType.Beer;

		public override void Spawn()
		{
			if ( string.IsNullOrEmpty( GetModelName() ) )
				SetModel( "models/barrels/square_wooden_box_gold.vmdl" );

			base.Spawn();
		}
	}
}
