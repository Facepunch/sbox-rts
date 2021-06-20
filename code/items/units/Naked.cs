using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Units
{
	[Library]
	public class Naked : BaseUnit
	{
		public override string Name => "Naked";
		public override string UniqueId => "unit.naked";
		public override string Description => "An angry Terry who seeks only one thing: blood!";
		public override Texture Icon => Texture.Load( "textures/rts/icons/naked.png" );
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150
		};
	}
}
