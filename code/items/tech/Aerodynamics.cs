using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class Aerodynamics : SpeedModifierTech<IDroneUnit>
	{
		public override string Name => "Aerodynamics";
		public override string UniqueId => "tech.aerodynamics";
		public override string Description => "Increases the speed of all drones by 15%.";
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 50
		};
		public override float Speed => 0.15f;
	}
}
