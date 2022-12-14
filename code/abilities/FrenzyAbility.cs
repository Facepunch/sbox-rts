using Sandbox;
using System.Collections.Generic;

namespace Facepunch.RTS
{
	[Library( "ability_frenzy" )]
	public class FrenzyAbility : BaseAbility
	{
		public override string Name => "Frenzy";
		public override string Description => "Weapon go brrr!";
		public override AbilityTargetType TargetType => AbilityTargetType.Self;
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "ui/icons/heal.png" );
		public override float Cooldown => 60f;
		public override HashSet<string> Dependencies => new()
		{
			"tech.syringes"
		};

		public override void OnFinished()
		{
			if ( Game.IsServer && TargetInfo.Target is UnitEntity unit )
			{
				unit.ApplyStatus<FrenzyStatus>( new ModifierData()
				{
					Duration = 8f,
					Modifier = 0.4f
				} );
			}

			base.OnFinished();
		}
	}
}
