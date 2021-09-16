using System;
using Sandbox;

namespace Facepunch.RTS
{
	/// <summary>
	/// Provides configuration for the in-game minimap
	/// </summary>
	[Library( "rts_minimap" )]
	[Hammer.EditorSprite( "editor/rts_minimap.vmat" )]
	public class MiniMapEntity : Entity
	{
		public static MiniMapEntity Instance { get; set; }

		[Property, FGDType( "png" )]
		public string TexturePath { get; set; }

		public override void ClientSpawn()
		{
			Instance = this;
			base.ClientSpawn();
		}
	}
}
