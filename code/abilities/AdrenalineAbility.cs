using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_adrenaline" )]
	public class AdrenalineAbility : BaseAbility
	{
		public override string Name => "Adrenaline";
		public override string Description => "Put your go faster stripes on.";
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public override Texture Icon => Texture.Load( "textures/rts/icons/heal.png" );
		public override float Cooldown => 60f;
		public override HashSet<string> Dependencies => new()
		{
			"tech.syringes"
		};

		public override void OnFinished()
		{
			if ( Host.IsServer && TargetInfo.Target is UnitEntity unit )
			{
				unit.ApplyStatus<BoostStatus>( new ModifierData()
				{
					Duration = 15f,
					Modifier = 0.4f
				} );
			}

			base.OnFinished();
		}
	}
}
