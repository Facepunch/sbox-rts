﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS.Abilities
{
	[Library]
	public class HealAbility : BaseAbility
	{
		public override string Name => "Heal";
		public override string UniqueId => "ability.heal";
		public override string Description => "Heal friendly units in range.";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 10f;
		public override float MaxDistance => 750f;
		public override float AreaOfEffect => 500f;
		public virtual float HealAmount => 10f;

		// TODO: This is just a test, it won't really require wheels.
		public override HashSet<string> Dependencies => new()
		{
			"tech.wheels"
		};

		public override void Use( Player player, UseAbilityInfo info )
		{
			var entities = Physics.GetEntitiesInSphere( info.origin, AreaOfEffect / 2f );

			foreach ( var entity in entities )
			{
				if ( entity is UnitEntity unit && unit.Player == player )
				{
					unit.GiveHealth( HealAmount );
				}
			}

			base.Use( player, info );
		}
	}
}