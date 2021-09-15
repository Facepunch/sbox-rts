using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class EmergencyExit : SpeedModifierTech<Medevac>
	{
		public override string Name => "Emergency Exit";
		public override string UniqueId => "tech.emergencyexit";
		public override string Description => "Increases the speed of Medevac aircraft by 30%.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 100,
			[ResourceType.Metal] = 25
		};
		public override float Speed => 0.30f;
	}
}
