using Sandbox;

namespace Facepunch.RTS
{
	[Library( "ability_killshot" )]
	public class KillshotAbility : BaseAbility
	{
		public override string Name => "Killshot";
		public override string Description => "Fire an accurate and high damage shot at a target.";
		public override AbilityTargetType TargetType => AbilityTargetType.Unit;
		public override AbilityTargetTeam TargetTeam => AbilityTargetTeam.Enemy;
		public override Texture Icon => Texture.Load( FileSystem.Mounted, "textures/rts/icons/heal.png" );
		public override float Cooldown => 60f;
		public override float MaxDistance => 1200f;

		public override void OnFinished()
		{
			if ( Host.IsServer && User is UnitEntity unit )
			{
				var target = TargetInfo.Target;
				var position = target.WorldSpaceBounds.Center;
				var direction = (position - unit.Position).Normal;

				unit.LookAtPosition( position );
				unit.Weapon.Dummy( position );

				var damageInfo = new DamageInfo
				{
					Damage = 70f,
					Attacker = unit,
					Flags = DamageFlags.Bullet,
					Weapon = unit.Weapon,
					Force = direction * 100f * 5f,
					Position = position
				};

				target.TakeDamage( damageInfo );
			}

			base.OnFinished();
		}
	}
}
