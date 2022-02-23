using Sandbox;
using System;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_heal" )]
	public class HealAbility : BaseAbility
	{
		public override string Name => "Heal";
		public override string Description => "Heal friendly units in range.";
		public override AbilityTargetType TargetType => AbilityTargetType.None;
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/heal.png" );
		public override float Cooldown => 30f;
		public override float MaxDistance => 750f;
		public override float AreaOfEffectRadius => 300f;

		public override void OnFinished()
		{
			if ( Host.IsServer )
			{
				Statuses.Apply<HealingStatus>( TargetInfo.Origin, AreaOfEffectRadius, new HealingData()
				{
					Interval = 0.1f,
					Duration = 5f,
					Amount = 0.5f
				}, CanHealUnit );
			}

			base.OnFinished();
		}

		private bool CanHealUnit( ISelectable target )
		{
			return (target.Player == User.Player);
		}
	}
}
