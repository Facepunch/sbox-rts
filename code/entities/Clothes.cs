using Sandbox;

namespace Facepunch.RTS
{
	public partial class Clothes : ModelEntity
	{
		public Clothes()
		{
			EnableShadowInFirstPerson = true;
			EnableHideInFirstPerson = true;
			AddCollisionLayer( CollisionLayer.Debris );
		}
	}
}

