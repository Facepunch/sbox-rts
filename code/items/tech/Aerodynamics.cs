using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Aerodynamics : SpeedModifierTech<BaseUnit>
	{
		public override string Name => "Aerodynamics";
		public override string UniqueId => "tech.aerodynamics";
		public override string Description => "Increases the speed of all drones by 15%.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 10;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 150
		};
		public override float Speed = 0.15f;
		public override string Tag => "drone";
	}
}
