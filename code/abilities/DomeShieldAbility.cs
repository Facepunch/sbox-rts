using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "ability_dome_shield" )]
	public class DomeShieldAbility : BaseAbility
	{
		public override string Name => "Dome Shield";
		public override string Description => "Deploy a shield around this unit that absords damage to all units within it.";
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 120f;
		public override float AreaOfEffectRadius => 300f;
		public override Dictionary<ResourceType, int> Costs => new()
		{
			[ResourceType.Plasma] = 20
		};
		public override HashSet<string> Dependencies => new()
		{
			"tech.extraction"
		};

		public override void OnFinished()
		{
			base.OnFinished();

			if ( Host.IsClient ) return;
			if ( User is not UnitEntity unit ) return;

			var shield = new DomeShieldEntity
			{
				Position = unit.Position
			};

			shield.SetParent( unit );
			shield.Setup( unit, 150f, AreaOfEffectRadius, 30f );
		}
	}
}
