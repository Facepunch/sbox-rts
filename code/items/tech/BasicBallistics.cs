using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class BasicBallistics : WeaponDamageTech<IBallisticsWeapon>
	{
		public override string Name => "Basic Ballistics";
		public override string UniqueId => "tech.basicballistics";
		public override string Description => "Increases the attack power of projectiles by +1.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 300,
			[ResourceType.Metal] = 200
		};
		public override int DamageModifier => 1;
	}
}
