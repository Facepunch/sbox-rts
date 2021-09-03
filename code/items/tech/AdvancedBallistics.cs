using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class AdvancedBallistics : WeaponDamageTech<IBallisticsWeapon>
	{
		public override string Name => "Advanced Ballistics";
		public override string UniqueId => "tech.advancedballistics";
		public override string Description => "Increases the attack power of projectiles by +1.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override bool AlwaysShowInList => false;
		public override int BuildTime => 80;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 200,
			[ResourceType.Metal] = 100
		};
		public override int DamageModifier => 1;
		public override HashSet<string> Dependencies => new()
		{
			"tech.basicballistics"
		};
	}
}
