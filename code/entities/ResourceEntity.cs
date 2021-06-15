using Sandbox;
using System;

namespace RTS
{
	public partial class ResourceEntity : ModelEntity
	{
		public virtual ResourceType Resource => ResourceType.Stone;
		public virtual float GatherTime => 1f;
		public virtual int MaxCarry => 10;

		[Property( Help = "How much of this resource there is left to take." )]
		[Net] public int Stock { get; set; } = 250;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Transmit = TransmitType.Always;

			// Let's make sure there is stock.
			if ( Stock == 0 ) Stock = 250;
		}
	}
}
