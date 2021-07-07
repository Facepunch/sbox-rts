using Sandbox;

namespace Gamelib.FlowFields.Entities
{
	[Library( "flowfield_blocker" )]
	[Hammer.AutoApplyMaterial( "materials/gamelib/flowfields/blocker.vmat" )]
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
