using Sandbox;

namespace Gamelib.FlowFields.Entities
{

	/// <summary>
	/// Blocks off areas that players should not be able to access.
	/// </summary>
	[Library( "flowfield_blocker" )]
	[Hammer.AutoApplyMaterial( "materials/rts/hammer/flowfield_blocker.vmat" )]
	[Hammer.EntityTool( "Blocker", "FlowField" )]
	[Hammer.Solid]
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
