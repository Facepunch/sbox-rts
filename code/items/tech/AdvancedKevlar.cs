using Facepunch.RTS.Units;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Tech
{
	[Library]
	public class AdvancedKevlar : ResistanceModifierTech<BaseUnit>
	{
		public override string Name => "Advanced Kevlar";
		public override string UniqueId => "tech.advancedkevlar";
		public override string Description => "Increases the bullet resistance of all units by 10%.";
		public override Texture Icon => Texture.Load( "textures/rts/icons/wheels.png" );
		public override bool AlwaysShowInList => false;
		public override int BuildTime => 60;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Beer] = 150,
			[ResourceType.Metal] = 100
		};
		public override Dictionary<string,float> ResistanceModifiers => new()
		{
			["resistance.bullet"] = 0.1f
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.kevlararmor"
		};
	}
}
