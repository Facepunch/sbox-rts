using Sandbox;
using SandboxEditor;

namespace Gamelib.FlowFields.Entities
{

	/// <summary>
	/// Blocks off areas that players should not be able to access.
	/// </summary>
	[Library( "flowfield_blocker" )]
	[AutoApplyMaterial( "materials/rts/hammer/flowfield_blocker.vmat" )]
	[Title( "Flow Field Blocker" )]
	[Solid]
	public class FlowFieldBlocker : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			SetInteractsAs( CollisionLayer.PLAYER_CLIP );
			SetupPhysicsFromModel( PhysicsMotionType.Static, true );

			Transmit = TransmitType.Never;
		}
	}
}
