using System;
using SandboxEditor;
using Sandbox;

namespace Facepunch.RTS
{
	/// <summary>
	/// Provides configuration for the in-game minimap
	/// </summary>
	[Library( "rts_minimap" )]
	[Title( "MiniMap Config" )]
	[EditorSprite( "editor/rts_minimap.vmat" )]
	public partial class MiniMapEntity : Entity
	{
		public static MiniMapEntity Instance { get; set; }

		[Net, Property, ResourceType( "png" )]
		public string TexturePath { get; set; }

		public override void Spawn()
		{
			Transmit = TransmitType.Always;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			Instance = this;

			base.ClientSpawn();
		}
	}
}
