using Sandbox;
using SandboxEditor;

namespace Facepunch.RTS
{

	/// <summary>
	/// Blocks the ability to build inside the volume, but still allows units to pass through
	/// </summary>
	[Library( "rts_building_blocker" )]
	[AutoApplyMaterial( "materials/rts/hammer/building_blocker.vmat" )]
	[Title( "Building Blocker" )]
	[Solid]
	public class BuildingBlocker : BaseTrigger
	{
		public override void Spawn()
		{
			base.Spawn();

			EnableDrawing = false;
			Transmit = TransmitType.Always;
		}
	}
}
