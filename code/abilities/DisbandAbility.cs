using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.RTS
{
	[Library( "ability_disband" )]
	public class DisbandAbility : BaseAbility
	{
		public override string Name => "Disband";
		public override string Description => "Delete this unit and free up population to create new ones.";
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 0f;

		public override bool IsAvailable()
		{
			if ( User is UnitEntity unit )
				return (unit.LastDamageTime >= 5);
			else
				return false;
		}

		public override void OnFinished()
		{
			base.OnFinished();

			if ( Host.IsClient ) return;
			if ( User is not UnitEntity unit ) return;

			if ( unit.LastDamageTime >= 5 )
			{
				unit.Kill();
			}
		}
	}
}
