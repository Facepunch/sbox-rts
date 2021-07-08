using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Abilities
{
	[Library( "ability_heal" )]
	public class HealAbility : BaseAbility
	{
		public override string Name => "Heal";
		public override string Description => "Heal friendly units in range.";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 10f;
		public override float MaxDistance => 750f;
		public override float AreaOfEffect => 500f;
		public virtual float HealAmount => 10f;

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				var targetInfo = TargetInfo;
				var entities = Physics.GetEntitiesInSphere( targetInfo.Origin, AreaOfEffect / 2f );

				foreach ( var entity in entities )
				{
					if ( entity is UnitEntity unit && unit.Player == User.Player )
					{
						unit.GiveHealth( HealAmount );
					}
				}
			}

			base.OnFinished();
		}
	}
}
