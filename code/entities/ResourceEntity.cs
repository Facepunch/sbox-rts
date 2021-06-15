using Sandbox;
using System;

namespace RTS
{
	public partial class ResourceEntity : ModelEntity
	{
		[Property( Help = "How much of this resource there is to take." )]
		public int Stock { get; set; } = 250;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Transmit = TransmitType.Always;
		}
	}
}
